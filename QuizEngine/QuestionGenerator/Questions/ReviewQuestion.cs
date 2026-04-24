using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class ReviewQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 10;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var game = await GetRandomGameWithCriteria(
                _dbContext.Games
                    .Include(g => g.Reviews)
                    .Where(g => g.TotalReviews >= (int)difficulty && g.Reviews.Any())
            );

            if (game == null)
                return null;

            var reviews = game.Reviews;
            if (reviews == null || reviews.Count == 0)
                return null;

            var review = reviews[Random.Shared.Next(reviews.Count)];

            var wrongGames = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.TotalReviews >= (int)difficulty && g.GameId != game.GameId)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            var answers = new List<QuestionAnswerDto>();
            foreach (var g in wrongGames)
                answers.Add(new QuestionAnswerDto { Content = g.Name, IsValid = false });

            answers.Add(new QuestionAnswerDto { Content = game.Name, IsValid = true });

            var contentSnippet = review.Content.Length > 400 ? review.Content[..400] + "..." : review.Content;

            var question = new QuestionDto
            {
                Content = $"Which game is this review about?<br/><br/>{contentSnippet}",
                TimeToAnswer = TimeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
