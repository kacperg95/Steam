using DTO.Quiz;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entites;

namespace QuizEngine.QuestionGenerator.Questions
{
    public class MovieQuestion(AppDbContext dbContext) : QuestionGeneratorBase(dbContext)
    {
        protected override int TimeToAnswer => 60;
        public override int Weight => 15;

        public override async Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty)
        {
            var game = await GetRandomGameWithCriteria(
                _dbContext.Games
                    .AsNoTracking()
                    .Include(g => g.Movies)
                    .Where(g => g.TotalReviews >= (int)difficulty && g.Movies.Any())
            );

            if (game == null)
                return null;

            var movies = game.Movies;
            if (movies == null || movies.Count == 0)
                return null;

            var movie = movies[Random.Shared.Next(movies.Count)];

            // Get three wrong answer games
            var wrongGames = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.TotalReviews >= (int)difficulty && g.GameId != game.GameId)
                .OrderBy(_ => EF.Functions.Random())
                .Take(3)
                .ToListAsync();

            if (wrongGames.Count < 3)
                return null;

            var answers = new List<QuestionAnswerDto>();
            foreach (var g in wrongGames)
                answers.Add(new QuestionAnswerDto { Content = g.Name, IsValid = false, MediaUrl = g.HeaderImageUrl });

            answers.Add(new QuestionAnswerDto { Content = game.Name, IsValid = true, MediaUrl = game.HeaderImageUrl });

            var question = new QuestionDto
            {
                Content = "Which game is this video clip from?",
                VideoUrl = movie.VideoUrl,
                TimeToAnswer = TimeToAnswer,
                Answers = answers
            };

            return FinalizeQuestion(question);
        }
    }
}
