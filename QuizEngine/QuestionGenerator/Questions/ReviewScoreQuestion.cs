using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class ReviewScoreQuestion(AppDbContext dbContext) : ReviewMetricQuestionBase<double>(dbContext)
    {
        protected override Func<Game, double> MetricSelector => g => g.ReviewScore;
        public override int Weight => 2;

        protected override string QuestionText => "Which of the following games has the highest review score?";
    }
}
