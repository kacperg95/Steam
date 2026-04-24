using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class ScreenshotQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 10;
        public override int Weight => 10;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var game = await _dbContext
                .Games
                .AsNoTracking()
                .Include(g => g.Screenshots)
                .Where(g => g.TotalReviews >= (int)difficulty && g.Screenshots.Any())
                .OrderBy(_ => EF.Functions.Random())
                .FirstAsync();

            var screenshot = game.Screenshots.OrderBy(_ => Guid.NewGuid()).First();

            var question = new QuestionDto
            {
                Content = "Which game does this screenshot come from?",
                MediaUrl = screenshot.ImageUrl,
                Answers = []
            };

            question.Answers.Add(new QuestionAnswerDto { Content = game.Name, IsValid = true });

            var otherGames = await _dbContext.Games
                .Where(g => g.GameId != game.GameId && g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .Select(g => g.Name)
                .Take(3)
                .ToListAsync();

            foreach (var name in otherGames)
                    question.Answers.Add(new QuestionAnswerDto { Content = name, IsValid = false });

            return FinalizeQuestion(question);
        }
    }
}
