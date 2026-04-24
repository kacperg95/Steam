using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Repository.Services;
using Services;
using SteamApi;

namespace BackgroundServices
{
    public class GetLatelyPopularGames(IServiceProvider _serviceProvider, ILogger<GetLatelyPopularGames> _logger) : BackgroundService
    {
        public static bool IsRunning { get; private set; } = false;
        private const int MinimalReviews = 800;
        private readonly TimeSpan _period = TimeSpan.FromDays(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var _logScope = _logger.BeginScope(new Dictionary<string, object> { ["BackgroundWorkerName"] = nameof(GetLatelyPopularGames) });
            while (!stoppingToken.IsCancellationRequested)
            {
                while (GetFullListOfGames.IsRunning || UpdateGames.IsRunning)
                {
                    _logger.LogInformation("Waiting for other background workers to finish before starting daily topsellers sync");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }

                IsRunning = true;
                _logger.LogInformation("Initializing daily topsellers sync");

                try
                {
                    int page = 0;
                    bool pageWasEmpty;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _steamApiClient = scope.ServiceProvider.GetRequiredService<SteamApiClient>();
                            var _gameService = scope.ServiceProvider.GetRequiredService<GameService>();
                            var _mapper = scope.ServiceProvider.GetRequiredService<Mapper>();

                            var result = await _steamApiClient.GetGameIds(page, true);
                            if (result == null || !result.Any()) break;

                            var ids = result.Where(kv => kv.Value >= MinimalReviews).Select(kv => kv.Key).ToList();
                            var newIds = await _gameService.GetNonExistingGameIds(ids);
                            _logger.LogInformation("Page {Page}: {Total} games fetched, {Qualifying} meet minimal reviews threshold {NewCount} new games to add", page, result.Count, ids.Count, newIds.Count());
                            pageWasEmpty = !newIds.Any();

                            foreach (var id in newIds)
                            {
                                try
                                {
                                    var gameDto = await _steamApiClient.GetGame(id);
                                    if (gameDto == null)
                                    {
                                        _logger.LogWarning("Game ID: {Id} not found in Steam API", id);
                                        continue;
                                    }

                                    var game = _mapper.MapGame(gameDto);
                                    await _gameService.AddOrUpdateGame(game);
                                    _logger.LogInformation("Game ID: {Id}, Name: {Name} added", id, game.Name);
                                    await Task.Delay(1000, stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Unsuccesful get for game ID: {Id}", id);
                                }
                            }
                        }
                        page += 100;
                        await Task.Delay(pageWasEmpty ? 500 : 5000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetLatelyPopularGames error");
                }

                IsRunning = false;
                _logger.LogInformation("Weekly topsellers sync successful");
                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}

