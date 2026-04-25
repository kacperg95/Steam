using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class WrongDeveloperQuestion(AppDbContext dbContext) : OwnerRelationQuestionBase<Developer>(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 3;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateWrongForOwner(
                difficulty,
                d => d.DeveloperId,
                d => d.Name,
                "developer",
                takeOwnerGames: 3,
                timeToAnswer: TimeToAnswer);
        }
    }
}
