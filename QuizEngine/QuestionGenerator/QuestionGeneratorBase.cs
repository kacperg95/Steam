using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator
{
    public abstract class QuestionGeneratorBase(AppDbContext dbContext) : IQuestionGenerator
    {
        protected readonly AppDbContext _dbContext = dbContext;
        protected abstract int TimeToAnswer { get; }
        public virtual int Weight => 1;


        public abstract Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty);

        protected QuestionDto FinalizeQuestion(QuestionDto question)
        {
            question.Answers = question.Answers.OrderBy(_ => Random.Shared.Next()).ToList();
            question.TimeToAnswer = TimeToAnswer;
            return question;
        }

        protected async Task<TGame> GetRandomGameWithCriteria<TGame>(IQueryable<TGame> query)
            where TGame : class
        {
            var q = query.AsNoTracking();
            var count = await q.CountAsync();
            if (count == 0) throw new Exception("No games found");

            int index = Random.Shared.Next(0, count);
            return await q.Skip(index).FirstAsync();
        }
    }
}
