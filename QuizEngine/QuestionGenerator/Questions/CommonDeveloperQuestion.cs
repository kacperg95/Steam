using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CommonDeveloperQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 20;
        public override int Weight => 2;


        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var correctDeveloper = await _dbContext.Developers
                .AsNoTracking()
                .Include(d => d.Games)
                .Where(d => d.Games.Count(g => g.TotalReviews >= (int)difficulty) >= 4)
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (correctDeveloper == null)
                return null;

            var selectedGames = correctDeveloper.Games
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4)
                .ToList();

            var selectedGameIds = selectedGames.Select(g => g.GameId).ToArray();
            var selectedCount = selectedGameIds.Length;

            var wrongDevelopers = await _dbContext.Developers
                .AsNoTracking()
                .Include(d => d.Games)
                .Where(d => d.DeveloperId != correctDeveloper.DeveloperId &&
                            d.Games.Count(g => selectedGameIds.Contains(g.GameId)) < selectedCount)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (wrongDevelopers.Count < 3)
                return null;

            var answers = wrongDevelopers
                .Select(d => new QuestionAnswerDto { Content = d.Name, IsValid = false })
                .ToList();
            answers.Add(new QuestionAnswerDto { Content = correctDeveloper.Name, IsValid = true });

            return FinalizeQuestion(new QuestionDto
            {
                Content = $"What is the common <span class=\"bold\">developer</span> for all of these games: {string.Join(", ", selectedGames.Select(g => g.Name))}?",
                Answers = answers
            });
        }
    }
}
