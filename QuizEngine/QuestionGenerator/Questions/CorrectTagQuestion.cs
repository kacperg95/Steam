using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CorrectTagQuestion(AppDbContext dbContext) : CorrectRelationQuestionBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 3;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateForRelation<Tag>(
                difficulty,
                g => g.Tags,
                t => t.TagId,
                t => t.Name,
                "tags",
                minCommon: 3,
                maxCommon: 6,
                timeToAnswer: TimeToAnswer);
        }
    }
}
