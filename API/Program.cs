using API.Hubs;
using API.Middleware;
using BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using QuizEngine;
using Repository;
using Repository.Services;
using Serilog;
using Serilog.Events;
using Services;
using SteamApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:80")
    //.WriteTo.Seq("http://localhost:5341")
    .Enrich.WithProperty("Application", "Steam")
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


//builder.Services.AddCors(options => {
//    //options.AddPolicy("SignalRPolicy", policy => {
//    //    policy.WithOrigins("https://localhost:7257")
//    //          .AllowAnyHeader()
//    //          .AllowAnyMethod()
//    //          .AllowCredentials();

//    //options.AddPolicy("SignalRPolicy", policy =>
//    //{
//    //    policy.WithOrigins("https://steam-quiz.nonstack.dev")
//    //          .AllowAnyHeader()
//    //          .AllowAnyMethod()
//    //          .AllowCredentials();

//    //});
//});

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
    .Or<TimeoutRejectedException>()
    .WaitAndRetryAsync(4, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

builder.Services.AddHttpClient("SteamStore", client =>
{
    client.BaseAddress = new Uri("https://store.steampowered.com/");
    client.Timeout = Timeout.InfiniteTimeSpan;
})
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy);

builder.Services.AddHttpClient("SteamApi", client =>
{
    client.BaseAddress = new Uri("https://api.steampowered.com/");
    client.Timeout = Timeout.InfiniteTimeSpan;
})
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("ApiOptions"));

builder.Services.AddSignalR();

builder.Services.AddScoped<SteamApiClient>();
builder.Services.AddScoped<SteamScrapper>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<Mapper>();
builder.Services.AddScoped<SmartCensor>();

builder.Services.AddQuizEngine();

builder.Services.AddHostedService<GetFullListOfGames>();
builder.Services.AddHostedService<UpdateGames>();
builder.Services.AddHostedService<GetLatelyPopularGames>();


try
{
    Log.Information("Starting web host");
    var app = builder.Build();

    app.UseForwardedHeaders();
    app.UseMiddleware<ExceptionLoggingMiddleware>();
    app.UseRouting();
    app.UseCors("SignalRPolicy");
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapControllers();
    app.MapHub<QuizHub>("/quizhub");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
