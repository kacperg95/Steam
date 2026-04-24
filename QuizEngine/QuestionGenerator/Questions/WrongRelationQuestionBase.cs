using System.Linq.Expressions;
using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public abstract class WrongRelationQuestionBase(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected async Task<QuestionDto?> GenerateForRelation<TEntity>(
            RoomDifficultyDto difficulty,
            Expression<Func<Game, IEnumerable<TEntity>>> navigationSelector,
            Func<TEntity, int> idSelector,
            Func<TEntity, string> nameSelector,
            string relationDisplayName,
            int minCommon = 2,
            int maxCommon = 4,
            int maxSeedGames = 10,
            int timeToAnswer = 30)
            where TEntity : class
        {
            var seedGames = await _dbContext.Games
                .AsNoTracking()
                .Include(navigationSelector)
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .Take(maxSeedGames)
                .ToListAsync();

            if (seedGames.Count == 0)
                return null;

            var navFunc = navigationSelector.Compile();

            var candidateGames = await _dbContext.Games
                .AsNoTracking()
                .Include(navigationSelector)
                .Where(g => g.TotalReviews >= (int)difficulty)
                .ToListAsync();

            foreach (var seedGame in seedGames)
            {
                var seedNav = navFunc(seedGame).ToList();
                if (seedNav.Count < minCommon) continue;

                int maxTake = Math.Min(maxCommon, seedNav.Count);
                int takeCount = Random.Shared.Next(minCommon, maxTake + 1);
                var combo = seedNav.OrderBy(_ => Guid.NewGuid()).Take(takeCount).ToList();
                var comboIds = combo.Select(idSelector).ToList();

                var matchingGames = candidateGames
                    .Where(g => g.GameId != seedGame.GameId && comboIds.All(id => navFunc(g).Any(x => idSelector(x) == id)))
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(2)
                    .ToList();

                if (matchingGames.Count < 2)
                    continue;

                matchingGames.Add(seedGame);

                var nonMatching = candidateGames
                    .Where(g => !matchingGames.Select(x => x.GameId).Contains(g.GameId) && !comboIds.All(id => navFunc(g).Any(x => idSelector(x) == id)))
                    .OrderBy(_ => Random.Shared.Next())
                    .FirstOrDefault();

                if (nonMatching == null)
                    continue;

                var answers = new List<QuestionAnswerDto>();
                foreach (var g in matchingGames)
                        answers.Add(new QuestionAnswerDto { Content = g.Name, IsValid = false });

                    answers.Add(new QuestionAnswerDto { Content = nonMatching.Name, IsValid = true });

                var question = new QuestionDto
                {
                    Content = $"Which of the following games does <span class=\"bold\">NOT</span> have the following <span class=\"bold\">{relationDisplayName}</span>: {string.Join(", ", combo.Select(c => nameSelector(c)))}?",
                    TimeToAnswer = timeToAnswer,
                    Answers = answers
                };

                return FinalizeQuestion(question);
            }

            return null;
        }
    }
}
