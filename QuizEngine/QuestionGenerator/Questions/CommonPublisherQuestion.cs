using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class CommonPublisherQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var correctPublisher = await _dbContext.Publishers
                .AsNoTracking()
                .Include(p => p.Games)
                .Where(p => p.Games.Count(g => g.TotalReviews >= (int)difficulty) >= 4)
                .OrderBy(_ => EF.Functions.Random())
                .FirstOrDefaultAsync();

            if (correctPublisher == null)
                return null;

            var selectedGames = correctPublisher.Games
                .Where(g => g.TotalReviews >= (int)difficulty)
                .OrderBy(_ => Guid.NewGuid())
                .Take(4)
                .ToList();

            var selectedGameIds = selectedGames.Select(g => g.GameId).ToArray();
            var selectedCount = selectedGameIds.Length;

            var wrongPublishers = await _dbContext.Publishers
                .AsNoTracking()
                .Include(p => p.Games)
                .Where(p => p.PublisherId != correctPublisher.PublisherId &&
                            p.Games.Count(g => selectedGameIds.Contains(g.GameId)) < selectedCount)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (wrongPublishers.Count < 3)
                return null;

            var answers = wrongPublishers
                .Select(p => new QuestionAnswerDto { Content = p.Name, IsValid = false })
                .ToList();
            answers.Add(new QuestionAnswerDto { Content = correctPublisher.Name, IsValid = true });

            return FinalizeQuestion(new QuestionDto
            {
                Content = $"What is the common <span class=\"bold\">publisher</span> for all of these games: {string.Join(", ", selectedGames.Select(g => g.Name))}?",
                Answers = answers
            });
        }
    }
}
