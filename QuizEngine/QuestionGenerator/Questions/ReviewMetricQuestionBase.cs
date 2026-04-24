using System;
using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public abstract class ReviewMetricQuestionBase<TMetric>(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
        where TMetric : notnull
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 5;

        protected abstract Func<Game, TMetric> MetricSelector { get; }

        protected abstract string QuestionText { get; }

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var games = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .Take(4)
                .ToListAsync();

            if (games == null || games.Count < 2)
                return null;

            var correct = games.OrderByDescending(MetricSelector).First();

            var answers = games.Select(g =>
            {
                var metricVal = MetricSelector(g);
                string formatted;

                if (typeof(TMetric) == typeof(int))
                {
                    var v = Convert.ToInt32(metricVal);
                    formatted = v >= 1000 ? $"{v / 1000}k" : v.ToString();
                }
                else if (typeof(TMetric) == typeof(double))
                {
                    var d = Convert.ToDouble(metricVal);
                    formatted = d.ToString("0.#") + "%";
                }
                else
                {
                    formatted = metricVal?.ToString() ?? string.Empty;
                }

                return new QuestionAnswerDto
                {
                    Content = g.Name,
                    IsValid = g.GameId == correct.GameId,
                    PostAnswerContent = $"{g.Name} ({formatted})"
                };
            }).ToList();

            var question = new QuestionDto
            {
                Content = QuestionText,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
