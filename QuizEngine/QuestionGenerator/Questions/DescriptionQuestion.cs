using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class DescriptionQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 5;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var game = await _dbContext.Games
                .Where(g => g.TotalReviews >= (int)difficulty && !string.IsNullOrEmpty(g.ShortDescriptionCensored))
                .OrderBy(_ => EF.Functions.Random())
                .Select(x => new
                {
                    x.GameId,
                    x.Name,
                    x.ShortDescriptionCensored
                })
                .FirstOrDefaultAsync();

            if (game == null)
                return null;

            var question = new QuestionDto
            {
                Content = $"Which game matches the following description?<br><br> {game.ShortDescriptionCensored ?? string.Empty}",
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
