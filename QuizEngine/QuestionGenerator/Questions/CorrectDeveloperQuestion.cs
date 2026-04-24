using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CorrectDeveloperQuestion(AppDbContext dbContext) : OwnerRelationQuestionBase<Developer>(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 2;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateCorrectForOwner(
                difficulty,
                d => d.DeveloperId,
                d => d.Name,
                "developer",
                timeToAnswer: TimeToAnswer);
        }
    }
}
