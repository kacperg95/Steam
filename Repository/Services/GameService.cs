using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Repository.Entites;
using System.Linq;

namespace Repository.Services
{
    public class GameService(AppDbContext _dbContext)
    {
        public async Task<IEnumerable<int>> GetNonExistingGameIds(IEnumerable<int> gameIds)
        {
            if (gameIds == null || !gameIds.Any())
                return [];

            var existingIds = await _dbContext.Games
                    .AsNoTracking()
                    .Where(g => gameIds.Contains(g.GameId))
                    .Select(g => g.GameId)
                    .ToHashSetAsync();

            return gameIds.Except(existingIds);
        }

        public async Task<List<int>> GetMostOutdatedGameIds(int count)
        {
            var lastUpdated = await _dbContext.Games
                .AsNoTracking()
                .OrderBy(g => g.LastUpdated)
                .Select(g => g.LastUpdated)
                .FirstOrDefaultAsync();

            if (DateTime.UtcNow - lastUpdated < TimeSpan.FromDays(3))
                return [];

            return await _dbContext.Games
                .AsNoTracking()
                .OrderBy(g => g.LastUpdated)
                .Select(g => g.GameId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetGameCount() => await _dbContext.Games.CountAsync();
        public async Task MarkLastUpdateAsUnsuccessful(int gameId)
        {
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
            if (game == null) return;
            game.LastUpdateSuccessful = false;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateGame(Game input)
        {

            var game = await _dbContext.Games
                .Include(g => g.Achievements)
                .Include(g => g.Reviews)
                .Include(g => g.Genres)
                .Include(g => g.Categories)
                .Include(g => g.Screenshots)
                .Include(g => g.Movies)
                .Include(g => g.Publishers)
                .Include(g => g.Developers)
                .Include(g => g.Tags)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.GameId == input.GameId);
                
            await AttachSharedEntities(input);

            if (game == null)
            {
                await _dbContext.Games.AddAsync(input);
            }
            else
            {
                _dbContext.Entry(game).CurrentValues.SetValues(input);

                foreach (var achievement in input.Achievements)
                {
                    var gameAchievement = game.Achievements.FirstOrDefault(x => x.ApiName == achievement.ApiName);

                    if (gameAchievement == null)
                    {
                        game.Achievements.Add(achievement);
                    }
                    else
                    {
                        gameAchievement.GlobalUnlockPercentage = achievement.GlobalUnlockPercentage;
                        gameAchievement.Description = achievement.Description;
                        gameAchievement.DisplayName = achievement.DisplayName;
                        gameAchievement.IconUrl = achievement.IconUrl;
                    }
                }

                foreach (var review in input.Reviews)
                {
                    var gameReview = game.Reviews.FirstOrDefault(x => x.Id == review.Id);
                    if (gameReview == null)
                        game.Reviews.Add(review);
                    else
                        _dbContext.Entry(gameReview).CurrentValues.SetValues(review);
                }

                foreach (var screenshot in input.Screenshots)
                {
                    var gameScreenshot = game.Screenshots.FirstOrDefault(x => x.ImageUrl == screenshot.ImageUrl);
                    if (gameScreenshot == null)
                        game.Screenshots.Add(screenshot);
                }

                foreach (var movie in input.Movies)
                {
                    var gameMovie = game.Movies.FirstOrDefault(x => x.Id == movie.Id);
                    if (gameMovie == null)
                        game.Movies.Add(movie);
                    else
                        _dbContext.Entry(gameMovie).CurrentValues.SetValues(movie);
                }

                foreach (var genre in input.Genres)
                {
                    var gameGenre = game.Genres.FirstOrDefault(x => x.GenreId == genre.GenreId);
                    if (gameGenre == null)
                        game.Genres.Add(genre);
                    else
                        _dbContext.Entry(gameGenre).CurrentValues.SetValues(genre);
                }

                foreach (var category in input.Categories)
                {
                    var gameCategory = game.Categories.FirstOrDefault(x => x.CategoryId == category.CategoryId);
                    if (gameCategory == null)
                        game.Categories.Add(category);
                    else
                        _dbContext.Entry(gameCategory).CurrentValues.SetValues(category);
                }

                foreach (var publisher in input.Publishers)
                {
                    var gamePublisher = game.Publishers.FirstOrDefault(x => x.Name == publisher.Name);
                    if (gamePublisher == null)
                        game.Publishers.Add(publisher);
                }

                foreach (var developer in input.Developers)
                {
                    var gameDeveloper = game.Developers.FirstOrDefault(x => x.Name == developer.Name);
                    if (gameDeveloper == null)
                        game.Developers.Add(developer);
                }

                foreach(var tag in input.Tags)
                {
                    var gameTag = game.Tags.FirstOrDefault(x => x.Name == tag.Name);
                    if (gameTag == null)
                        game.Tags.Add(tag);
                }   


                SyncOwnedCollection(game.Achievements, input.Achievements, (e, i) => e.ApiName == i.ApiName);
                SyncOwnedCollection(game.Reviews, input.Reviews, (e, i) => e.Id == i.Id);
                SyncOwnedCollection(game.Screenshots, input.Screenshots, (e, i) => e.ImageUrl == i.ImageUrl);
                SyncOwnedCollection(game.Movies, input.Movies, (e, i) => e.Id == i.Id);

                SyncSharedCollection(game.Genres, input.Genres, (e, i) => e.GenreId == i.GenreId);
                SyncSharedCollection(game.Categories, input.Categories, (e, i) => e.CategoryId == i.CategoryId);
                SyncSharedCollection(game.Publishers, input.Publishers, (e, i) => e.Name == i.Name);
                SyncSharedCollection(game.Developers, input.Developers, (e, i) => e.Name == i.Name);
                SyncSharedCollection(game.Tags, input.Tags, (e, i) => e.Name == i.Name);
            }


            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        private void SyncOwnedCollection<T>(ICollection<T> existing, ICollection<T> input, Func<T, T, bool> predicate) where T : class
        {
            var toRemove = existing.Where(e => !input.Any(i => predicate(e, i))).ToList();
            foreach (var item in toRemove)
                _dbContext.Set<T>().Remove(item);

        }

        private void SyncSharedCollection<T>(ICollection<T> existing, ICollection<T> input, Func<T, T, bool> predicate) where T : class
        {
            var toRemove = existing.Where(e => !input.Any(i => predicate(e, i))).ToList();
            foreach (var item in toRemove)
                existing.Remove(item);

        }

        private async Task AttachSharedEntities(Game input)
        {
            var existingGenres = await _dbContext.Genres.Where(g => input.Genres.Select(x => x.GenreId).Contains(g.GenreId)).ToListAsync();
            var nonExistingGenres = input.Genres.Where(g => !existingGenres.Select(x => x.GenreId).Contains(g.GenreId)).ToList();
            input.Genres = [.. existingGenres, .. nonExistingGenres];

            var existingCategories = await _dbContext.Categories.Where(g => input.Categories.Select(x => x.CategoryId).Contains(g.CategoryId)).ToListAsync();
            var nonExistingCategories = input.Categories.Where(g => !existingCategories.Select(x => x.CategoryId).Contains(g.CategoryId)).ToList();
            input.Categories = [.. existingCategories, .. nonExistingCategories];

            var existingDevelopers = await _dbContext.Developers.Where(g => input.Developers.Select(x => x.Name).Contains(g.Name)).ToListAsync();
            var nonExistingDevelopers = input.Developers.Where(g => !existingDevelopers.Select(x => x.Name).Contains(g.Name)).ToList();
            input.Developers = [.. existingDevelopers, .. nonExistingDevelopers];

            var existingPublishers = await _dbContext.Publishers.Where(g => input.Publishers.Select(x => x.Name).Contains(g.Name)).ToListAsync();
            var nonExistingPublishers = input.Publishers.Where(g => !existingPublishers.Select(x => x.Name).Contains(g.Name)).ToList();
            input.Publishers = [.. existingPublishers, .. nonExistingPublishers];

            var existingTags = await _dbContext.Tags.Where(g => input.Tags.Select(x => x.Name).Contains(g.Name)).ToListAsync();
            var nonExistingTags = input.Tags.Where(g => !existingTags.Select(x => x.Name).Contains(g.Name)).ToList();
            input.Tags = [.. existingTags, .. nonExistingTags]; 
        }

    }
}
