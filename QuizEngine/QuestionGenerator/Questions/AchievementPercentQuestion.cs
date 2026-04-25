using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class AchievementPercentQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 30;
        public override int Weight => 5;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var game = await GetRandomGameWithCriteria(
                _dbContext.Games.Where(g => g.TotalReviews >= (int)difficulty && g.Achievements.Any(a => !string.IsNullOrEmpty(a.Description))));

            var achievement = await _dbContext.Achievements
                .Where(a => a.GameId == game.GameId && !string.IsNullOrEmpty(a.Description))
                .OrderBy(_ => EF.Functions.Random()) 
                .FirstAsync();

            var question = new QuestionDto
            {
                Content = $"How many % of players achieved: <span class=\"bold\">{achievement.DisplayName}</span> in game <span class=\"bold\">{game.Name}</span>?<br><br>{achievement.Description}",
                MediaUrl = achievement.IconUrl,
                Answers = new List<QuestionAnswerDto>()
            };

            int correct = (int)Math.Round(achievement.GlobalUnlockPercentage);
            question.Answers.Add(new QuestionAnswerDto { Content = $"{correct}%", IsValid = true });

            while (question.Answers.Count < 4)
            {
                int offset = Random.Shared.Next(-25, 26);
                int wrong = Math.Clamp(correct + offset, 0, 100);
                string content = $"{wrong}%";

                if (question.Answers.All(a => a.Content != content))
                    question.Answers.Add(new QuestionAnswerDto { Content = content, IsValid = false });
            }

            return FinalizeQuestion(question);
        }
    }
}
