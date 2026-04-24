using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CorrectPublisherQuestion(AppDbContext dbContext) : OwnerRelationQuestionBase<Publisher>(dbContext)
    {
        protected override int TimeToAnswer => 30;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateCorrectForOwner(
                difficulty,
                p => p.PublisherId,
                p => p.Name,
                "publisher",
                timeToAnswer: TimeToAnswer);
        }
    }
}
