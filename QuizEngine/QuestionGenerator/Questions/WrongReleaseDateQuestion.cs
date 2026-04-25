using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class WrongReleaseDateQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 3;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var yearResult = await _dbContext.Games
                .Where(g => g.TotalReviews >= (int)difficulty && g.ReleaseDate.HasValue)
                .GroupBy(g => g.ReleaseDate!.Value.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .Where(x => x.Count >= 3)
                .OrderBy(_ => EF.Functions.Random())
                .FirstAsync();

            var gamesFromYear = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.ReleaseDate.HasValue && g.ReleaseDate.Value.Year == yearResult.Year && g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (gamesFromYear.Count < 3) return null;

            var otherGame = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.TotalReviews >= (int)difficulty && (!g.ReleaseDate.HasValue || g.ReleaseDate.Value.Year != yearResult.Year))
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (otherGame == null) return null;

            var answers = new List<QuestionAnswerDto>();
            foreach (var g in gamesFromYear)
                answers.Add(new QuestionAnswerDto { Content = g.Name, PostAnswerContent = $"{g.Name} ({yearResult.Year})", IsValid = false });

            answers.Add(new QuestionAnswerDto { Content = otherGame.Name, PostAnswerContent = $"{otherGame.Name} ({otherGame.ReleaseDate?.Year})", IsValid = true });

            var question = new QuestionDto
            {
                Content = $"Three of these games were released in <span class=\"bold\">{yearResult.Year}</span>. Which one was <span class=\"bold\">NOT</span> released in that year?",
                TimeToAnswer = TimeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
