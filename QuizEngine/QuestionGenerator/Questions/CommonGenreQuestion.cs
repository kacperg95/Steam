using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CommonGenreQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 2;


        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var correctGenre = await _dbContext.Genres
                .AsNoTracking()
                .Include(g => g.Games)
                .Where(g => g.Games.Count(game => game.TotalReviews >= (int)difficulty) >= 4)
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (correctGenre == null)
                return null;

            var selectedGames = correctGenre.Games
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4)
                .ToList();

            var selectedGameIds = selectedGames.Select(g => g.GameId).ToArray();
            var selectedCount = selectedGameIds.Length;

            var wrongGenres = await _dbContext.Genres
                .AsNoTracking()
                .Include(g => g.Games)
                .Where(g => g.GenreId != correctGenre.GenreId &&
                            g.Games.Count(game => selectedGameIds.Contains(game.GameId)) < selectedCount)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (wrongGenres.Count < 3)
                return null;

            var answers = wrongGenres
                .Select(g => new QuestionAnswerDto { Content = g.Name, IsValid = false })
                .ToList();
            answers.Add(new QuestionAnswerDto { Content = correctGenre.Name, IsValid = true });

            return FinalizeQuestion(new QuestionDto
            {
                Content = $"What is the common <span class=\"bold\">genre</span> for all of these games: {string.Join(", ", selectedGames.Select(g => g.Name))}?",
                Answers = answers
            });
        }
    }
}
