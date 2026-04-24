using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class WrongTagQuestion(AppDbContext dbContext) : WrongRelationQuestionBase(dbContext)
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
                minCommon: 2,
                maxCommon: 4,
                maxSeedGames: 10,
                timeToAnswer: TimeToAnswer);
        }
    }
}
