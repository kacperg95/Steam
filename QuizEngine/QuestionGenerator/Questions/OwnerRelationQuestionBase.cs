using System.Reflection;
using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public abstract class OwnerRelationQuestionBase<TOwner>(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
        where TOwner : class
    {
        protected async Task<QuestionDto?> GenerateWrongForOwner(
            RoomDifficultyDto difficulty,
            Func<TOwner, int> idSelector,
            Func<TOwner, string> nameSelector,
            string ownerDisplayName,
            int takeOwnerGames = 3,
            int timeToAnswer = 20)
        {
            var owners = await _dbContext.Set<TOwner>()
                .AsNoTracking()
                .Include("Games")
                .ToListAsync();

            var owner = owners
                .Where(o =>
                {
                    var gamesProp = o!.GetType().GetProperty("Games");
                    if (gamesProp == null) return false;
                    var games = (IEnumerable<Game>?)gamesProp.GetValue(o) ?? Enumerable.Empty<Game>();
                    return games.Count() >= 3 && games.Any(g => g.TotalReviews > (int)difficulty);
                })
                .OrderBy(_ => Random.Shared.Next())
                .FirstOrDefault();

            if (owner == null)
                return null;

            var gamesPropOwner = owner.GetType().GetProperty("Games")!;
            var ownerGames = ((IEnumerable<Game>)gamesPropOwner.GetValue(owner)!).OrderBy(_ => Guid.NewGuid()).Take(takeOwnerGames).Select(g => g.Name).ToList();

            var relationProp = typeof(Game).GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments()[0] == typeof(TOwner));

            IQueryable<Game> query = _dbContext.Games;
            if (relationProp != null)
                query = query.Include(relationProp.Name);

            var candidateGames = await query
                .AsNoTracking()
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .ToListAsync();

            var ownerId = idSelector(owner);

            Game? nonAssociated = null;
            if (relationProp != null)
            {
                nonAssociated = candidateGames
                    .FirstOrDefault(g =>
                    {
                        var list = (IEnumerable<TOwner>?)relationProp.GetValue(g) ?? Enumerable.Empty<TOwner>();
                        return !list.Any(x => idSelector(x) == ownerId);
                    });
            }
            else
            {
                var ownerGameNames = new HashSet<string>(ownerGames);
                nonAssociated = candidateGames.FirstOrDefault(g => !ownerGameNames.Contains(g.Name));
            }

            if (nonAssociated == null)
                return null;

            var question = new QuestionDto
            {
                Content = $"Which of the following games does <span class=\"bold\">NOT</span> belong to {ownerDisplayName}: <span class=\"bold\">{nameSelector(owner)}</span>?",
                TimeToAnswer = timeToAnswer,
                Answers = new List<QuestionAnswerDto>()
            };

            foreach (var name in ownerGames)
                question.Answers.Add(new QuestionAnswerDto { Content = name, IsValid = false });

            question.Answers.Add(new QuestionAnswerDto { Content = nonAssociated.Name, IsValid = true });

            return FinalizeQuestion(question);
        }

        protected async Task<QuestionDto?> GenerateCorrectForOwner(
            RoomDifficultyDto difficulty,
            Func<TOwner, int> idSelector,
            Func<TOwner, string> nameSelector,
            string ownerDisplayName,
            int timeToAnswer = 30)
        {
            var owners = await _dbContext.Set<TOwner>()
                .Include("Games")
                .AsNoTracking()
                .ToListAsync();

            var owner = owners
                .Where(o =>
                {
                    var gamesProp = o!.GetType().GetProperty("Games");
                    if (gamesProp == null) return false;
                    var games = (IEnumerable<Game>?)gamesProp.GetValue(o) ?? Enumerable.Empty<Game>();
                    return games.Count() >= 3 && games.Any(g => g.TotalReviews > (int)difficulty);
                })
                .OrderBy(_ => Random.Shared.Next())
                .FirstOrDefault();

            if (owner == null)
                return null;

            var gamesPropOwner = owner.GetType().GetProperty("Games")!;
            var ownerGames = ((IEnumerable<Game>)gamesPropOwner.GetValue(owner)!).OrderBy(_ => Guid.NewGuid()).ToList();
            var correctGame = ownerGames.FirstOrDefault(g => g.TotalReviews >= (int)difficulty) ?? ownerGames.FirstOrDefault();

            if (correctGame == null)
                return null;

            var relationProp = typeof(Game).GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments()[0] == typeof(TOwner));

            IQueryable<Game> query = _dbContext.Games;
            if (relationProp != null)
                query = query.Include(relationProp.Name);

            var candidateGames = await query
                .Where(g => g.TotalReviews >= (int)difficulty && g.GameId != correctGame.GameId)
                .OrderBy(_ => EF.Functions.Random())
                .ToListAsync();

            var ownerId = idSelector(owner);

            var nonMatching = new List<Game>();
            if (relationProp != null)
            {
                foreach (var g in candidateGames)
                {
                    var list = (IEnumerable<TOwner>?)relationProp.GetValue(g) ?? Enumerable.Empty<TOwner>();
                    if (!list.Any(x => idSelector(x) == ownerId))
                    {
                        nonMatching.Add(g);
                        if (nonMatching.Count >= 3) break;
                    }
                }
            }
            else
            {
                var ownerGameIds = new HashSet<int>(((IEnumerable<Game>)gamesPropOwner.GetValue(owner)!).Select(x => x.GameId));
                foreach (var g in candidateGames)
                {
                    if (!ownerGameIds.Contains(g.GameId))
                    {
                        nonMatching.Add(g);
                        if (nonMatching.Count >= 3) break;
                    }
                }
            }

            if (nonMatching.Count < 3)
                return null;

            var answers = new List<QuestionAnswerDto>();
            foreach (var g in nonMatching)
                answers.Add(new QuestionAnswerDto { Content = g.Name, IsValid = false });

            answers.Add(new QuestionAnswerDto { Content = correctGame.Name, IsValid = true });

            var question = new QuestionDto
            {
                Content = $"Which of the following games <span class=\"bold\">DOES</span> belong to {ownerDisplayName}: <span class=\"bold\">{nameSelector(owner)}</span>?",
                TimeToAnswer = timeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
