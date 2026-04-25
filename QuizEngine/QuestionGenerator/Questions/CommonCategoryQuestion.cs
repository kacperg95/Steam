using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CommonCategoryQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var correctCategory = await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Games)
                .Where(c => c.Games.Count(g => g.TotalReviews >= (int)difficulty) >= 4)
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (correctCategory == null)
                return null;

            var selectedGames = correctCategory.Games
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4)
                .ToList();

            var selectedGameIds = selectedGames.Select(g => g.GameId).ToHashSet();

            var wrongCategories = (await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Games)
                .OrderBy(_ => EF.Functions.Random())
                .Where(c => c.CategoryId != correctCategory.CategoryId)
                .Where(c => !selectedGameIds.All(gid => c.Games.Any(g => g.GameId == gid)))
                .Take(3)
                .ToListAsync());

            if (wrongCategories.Count < 3)
                return null;

            var answers = wrongCategories
                .Select(c => new QuestionAnswerDto { Content = c.Name, IsValid = false })
                .ToList();
            answers.Add(new QuestionAnswerDto { Content = correctCategory.Name, IsValid = true });

            return FinalizeQuestion(new QuestionDto
            {
                Content = $"What is the common <span class=\"bold\">category</span> for all of these games: {string.Join(", ", selectedGames.Select(g => g.Name))}?",
                Answers = answers
            });
        }
    }
}
