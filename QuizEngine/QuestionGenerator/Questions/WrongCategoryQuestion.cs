using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class WrongCategoryQuestion(AppDbContext dbContext) : WrongRelationQuestionBase(dbContext)
    {
        protected override int TimeToAnswer => 30;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateForRelation<Category>(
                difficulty,
                g => g.Categories,
                c => c.CategoryId,
                c => c.Name,
                "categories",
                minCommon: 2,
                maxCommon: 3,
                maxSeedGames: 10,
                timeToAnswer: TimeToAnswer);
        }
    }
}
