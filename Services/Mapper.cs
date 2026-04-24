using DTO.SteamApis;
using Repository.Entites;
using System.Globalization;

namespace Services
{
    public class Mapper(SmartCensor _smartCensor)
    {
        public Game MapGame(FullGameDetailsDto input)
        {
            Game game = new()
            {
                GameId = input.Id,
                Name = input.Name,
                ShortDescription = input.ShortDescription,
                ShortDescriptionCensored = _smartCensor.Censor(input.ShortDescription, input.Name),
                HeaderImageUrl = input.HeaderImageUrl,
                ReleaseDate = ParseReleaseDate(input.ReleaseDate),
                PositiveReviews = input.PositiveReviews,
                NegativeReviews = input.NegativeReviews,
                TotalReviews =  input.TotalReviews,
                ReviewScore = input.ReviewScore,
                LastUpdated = DateTime.UtcNow,
                LastUpdateSuccessful = true,
                Developers = [.. input.Developers.Select(d => new Developer { Name = d })],
                Publishers = [.. input.Publishers.Select(p => new Publisher { Name = p })],
                Tags = [.. input.Tags.Select(t => new Tag { Name = t })],
                Genres = [.. input.Genres.Select(g => new Genre { GenreId = g.Id, Name = g.Name })],
                Categories = [.. input.Categories.Select(c => new Category { CategoryId = c.Id, Name = c.Name })],
                Movies = [.. input.Movies.Select(m => new Movie { Id = m.Id, VideoUrl = m.Url, GameId = input.Id })],
                Screenshots = [.. input.Screenshots.Select(s => new Screenshot { ImageUrl = s.Url, GameId = input.Id })],
                Achievements = [.. input.Achievements.Select(a => new Achievement
                {
                    ApiName = a.Name,
                    DisplayName = a.DisplayName,
                    Description = a.Description,
                    IconUrl = a.IconUrl,
                    GlobalUnlockPercentage = a.PercentAchieved,
                    GameId = input.Id
                })],
                Reviews = [.. input.Reviews.Select(r => new Review
                {
                    Id = r.Id,
                    Author = r.Author,
                    IsRecommended = r.VotedUp,
                    Playtime = r.Playtime,
                    Content = _smartCensor.Censor(r.Content, input.Name),
                    GameId = input.Id
                })]
            };

            return game;
        }


        public FullGameDetailsDto MapGame(SteamStoreGameDetailsDto steamStoreDetails, SteamPoweredAchievementDescriptionsDto? steamAchievements, SteamPoweredAchievementPercentagesDto? steamAchievementsPercentages, SteamStoreReviewCountResponseDto steamReviewCount, SteamStoreReviewsDto steamReviews, List<string> tags)
        {
            var game = new FullGameDetailsDto();
            game.Id = steamStoreDetails.Data.AppId;
            game.Name = steamStoreDetails.Data.Name;
            game.ShortDescription = steamStoreDetails.Data.ShortDescription;
            game.HeaderImageUrl = steamStoreDetails.Data.HeaderImageUrl;
            game.Developers = steamStoreDetails.Data.Developers.Distinct().ToList();
            game.Publishers = steamStoreDetails.Data.Publishers.Distinct().ToList();
            game.Tags = tags;
            game.ReleaseDate = steamStoreDetails.Data.ReleaseDate.Date;
            game.PositiveReviews = steamReviewCount.QuerySummary.TotalPositive;
            game.NegativeReviews = steamReviewCount.QuerySummary.TotalNegative;
            game.TotalReviews = steamReviewCount.QuerySummary.TotalReviews;
            game.ReviewScore = game.TotalReviews > 0 ? (double)game.PositiveReviews / game.TotalReviews * 100 : 0;

            if (steamAchievements != null && steamAchievementsPercentages != null && steamAchievements.Game.AvailableGameStats.Achievements.Count != 0)
            {
                game.Achievements = steamAchievements.Game.AvailableGameStats.Achievements.Select(a =>
                {
                    var percentage = steamAchievementsPercentages!.AchievementPercentages.Achievements.FirstOrDefault(p => p.Name == a.Name)?.Percent ?? 0;

                    return new FullGameDetailsDto.FullGameAchievementDto
                    {
                        Name = a.Name,
                        DisplayName = a.DisplayName,
                        Description = a.Description,
                        IconUrl = a.IconUrl,
                        PercentAchieved = percentage
                    };
                }).ToList();
            }
            else
            {
                game.Achievements = [];
            }

            game.Categories = steamStoreDetails.Data.Categories.Select(c => new FullGameDetailsDto.FullGameDetailsCategoryOrGenreDto
            {
                Id = c.Id,
                Name = c.Description
            }).ToList();

            game.Genres = steamStoreDetails.Data.Genres.Select(g => new FullGameDetailsDto.FullGameDetailsCategoryOrGenreDto
            {
                Id = g.Id,
                Name = g.Description
            }).ToList();

            game.Reviews = steamReviews.Reviews.Select(r => new FullGameDetailsDto.FullGameReviewDto
            {
                Id = r.RecommendationId,
                Author = r.Author.PersonaName,
                Content = r.Content,
                VotedUp = r.VotedUp,
                Playtime = r.Author.Playtime
            }).ToList();

            game.Movies = steamStoreDetails.Data.Movies.Select(m => new FullGameDetailsDto.FullGameMovieOrScreenshotDto
            {
                Id = m.Id,
                Url = m.Url
            }).ToList();

            game.Screenshots = steamStoreDetails.Data.Screenshots.Select(s => new FullGameDetailsDto.FullGameMovieOrScreenshotDto
            {
                Id = s.Id,
                Url = s.Url
            }).ToList();

            return game;

        }
        private static DateTime? ParseReleaseDate(string date) => string.IsNullOrEmpty(date) ? null : DateTime.ParseExact(date, "d MMM, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }
}
