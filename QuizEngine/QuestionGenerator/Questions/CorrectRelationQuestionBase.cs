using System.Linq.Expressions;
using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public abstract class CorrectRelationQuestionBase(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected async Task<QuestionDto?> GenerateForRelation<TEntity>(
            RoomDifficultyDto difficulty,
            Expression<Func<Game, IEnumerable<TEntity>>> navigationSelector,
            Func<TEntity, int> idSelector,
            Func<TEntity, string> nameSelector,
            string relationDisplayName,
            int minCommon = 2,
            int maxCommon = 4,
            int timeToAnswer = 30)
            where TEntity : class
        {
            var candidateGames = await _dbContext.Games
                .AsNoTracking()
                .Include(navigationSelector)
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .Take(100)                 
                .ToListAsync();            

            var correctGame = candidateGames
                .FirstOrDefault(g => navigationSelector.Compile()(g).Count() >= minCommon);

            if(correctGame == null)
                return null;

            var navFunc = navigationSelector.Compile();
            var seedNav = navFunc(correctGame).ToList();
            int maxTake = Math.Min(maxCommon, seedNav.Count);
            int takeCount = Random.Shared.Next(minCommon, maxTake + 1);
            var combo = seedNav.OrderBy(_ => Guid.NewGuid()).Take(takeCount).ToList();
            var comboIds = combo.Select(idSelector).ToList();

            var nonMatchingGames = candidateGames
                .Where(g => !comboIds.All(id => navFunc(g).Any(x => idSelector(x) == id)))
                .Take(3)
                .ToList();

            if (nonMatchingGames.Count < 3)
                return null;

            var answers = new List<QuestionAnswerDto>();
            foreach (var g in nonMatchingGames)
                answers.Add(new QuestionAnswerDto { Content = g.Name, IsValid = false });

            answers.Add(new QuestionAnswerDto { Content = correctGame.Name, IsValid = true });

            var question = new QuestionDto
            {
                Content = $"Which of the following games <span class=\"bold\">DOES</span> have these <span class=\"bold\">{relationDisplayName}</span>: {string.Join(", ", combo.Select(c => nameSelector(c)))}?",
                TimeToAnswer = timeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
