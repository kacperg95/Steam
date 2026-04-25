using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class ReleaseQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 5;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var games = await _dbContext.Games
                .Where(g => g.TotalReviews >= (int)difficulty && g.ReleaseDate.HasValue)
                .OrderBy(_ => EF.Functions.Random())
                .Take(4)
                .Select(g => new
                {
                    g.GameId,
                    g.Name,
                    g.ReleaseDate
                })
                .ToListAsync();


            bool askYoungest = Random.Shared.Next(2) == 0;

            var correct = askYoungest
                ? games.OrderByDescending(g => g.ReleaseDate).FirstOrDefault()
                : games.OrderBy(g => g.ReleaseDate).FirstOrDefault();

            if (correct == null)
                return null;

            var answers = games.Select(g => new QuestionAnswerDto
            {
                Content = g.Name,
                IsValid = g.GameId == correct.GameId,
                PostAnswerContent = g.ReleaseDate.HasValue ? $"{g.Name} ({g.ReleaseDate.Value.Year})" : g.Name
            }).ToList();

            var question = new QuestionDto
            {
                Content = $"Which of the following games is the {(askYoungest ? "newest (most recently released)" : "oldest (released earliest)")}?",
                TimeToAnswer = TimeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
