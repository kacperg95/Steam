using DTO.Quiz;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class WrongGenreQuestion(AppDbContext dbContext) : WrongRelationQuestionBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 2;

        public override Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            return GenerateForRelation<Genre>(
                difficulty,
                g => g.Genres,
                c => c.GenreId,
                c => c.Name,
                "genres",
                minCommon: 1,
                maxCommon: 2,
                maxSeedGames: 10,
                timeToAnswer: TimeToAnswer);
        }
    }
}
