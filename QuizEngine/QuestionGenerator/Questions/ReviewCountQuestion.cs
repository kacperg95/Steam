using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class ReviewCountQuestion(AppDbContext dbContext) : ReviewMetricQuestionBase<int>(dbContext)
    {
        protected override Func<Game, int> MetricSelector => g => g.TotalReviews;

        protected override string QuestionText => "Which of the following games has the highest number of reviews?";
    }
}
