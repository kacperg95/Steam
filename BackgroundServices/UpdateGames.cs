using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Repository.Services;
using Services;
using SteamApi;

namespace BackgroundServices
{
    public class UpdateGames(IServiceProvider _serviceProvider, ILogger<UpdateGames> _logger) : BackgroundService
    {
        public static bool IsRunning { get; private set; } = false;
        private readonly TimeSpan _period = TimeSpan.FromDays(1);
        private const int PercentOfGamesToBeUpdated = 10;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var _logScope = _logger.BeginScope(new Dictionary<string, object> { ["BackgroundWorkerName"] = nameof(UpdateGames) });
            while (!stoppingToken.IsCancellationRequested)
            {
                while (GetFullListOfGames.IsRunning || GetLatelyPopularGames.IsRunning)
                {
                    _logger.LogInformation("Waiting for other background workers to finish before starting daily game sync");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }

                IsRunning = true;
                _logger.LogInformation("Initializing daily game sync");

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var gameService = scope.ServiceProvider.GetRequiredService<GameService>();
                        var steamApiClient = scope.ServiceProvider.GetRequiredService<SteamApiClient>();
                        var mapper = scope.ServiceProvider.GetRequiredService<Mapper>();

                        var totalGames = await gameService.GetGameCount();
                        _logger.LogInformation("Total games in database: {TotalGames}", totalGames);
                        if (totalGames > 0)
                        {
                            int count = Math.Max(1, (int)(totalGames * PercentOfGamesToBeUpdated / 100.0));
                            var idsToUpdate = await gameService.GetMostOutdatedGameIds(count);
                            _logger.LogInformation("Scheduled {Count} games for update ({Percent}% of total)", count, PercentOfGamesToBeUpdated);

                            foreach (var id in idsToUpdate)
                            {
                                try
                                {
                                    var gameDto = await steamApiClient.GetGame(id);
                                    if (gameDto == null)
                                    {
                                        await gameService.MarkLastUpdateAsUnsuccessful(id);
                                        _logger.LogWarning("Game ID: {Id} not found in Steam API", id);
                                        continue;
                                    }

                                    var game = mapper.MapGame(gameDto);
                                    await gameService.AddOrUpdateGame(game);
                                    _logger.LogInformation("Game ID: {Id}, Name: {Name} updated successfully", id, game.Name);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Unsuccesful get for game ID: {Id}", id);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UpdateGames error");
                }

                IsRunning = false;
                _logger.LogInformation("Daily game sync successful");
                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
