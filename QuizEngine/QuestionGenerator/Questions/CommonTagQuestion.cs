using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CommonTagQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 3;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var correctTag = await _dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Games)
                .Where(t => t.Games.Count(g => g.TotalReviews >= (int)difficulty) >= 4)
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (correctTag == null)
                return null;

            var selectedGames = correctTag.Games
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4)
                .ToList();

            var selectedGameIds = selectedGames.Select(g => g.GameId).ToArray();
            var selectedCount = selectedGameIds.Length;

            var wrongTags = await _dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Games)
                .Where(t => t.TagId != correctTag.TagId &&
                            t.Games.Count(g => selectedGameIds.Contains(g.GameId)) < selectedCount)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (wrongTags.Count < 3)
                return null;

            var answers = wrongTags
                .Select(t => new QuestionAnswerDto { Content = t.Name, IsValid = false })
                .ToList();
            answers.Add(new QuestionAnswerDto { Content = correctTag.Name, IsValid = true });

            return FinalizeQuestion(new QuestionDto
            {
                Content = $"What is the common <span class=\"bold\">tag</span> for all of these games: {string.Join(", ", selectedGames.Select(g => g.Name))}?",
                Answers = answers
            });
        }
    }
}
