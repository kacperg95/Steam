using DTO.Quiz;

namespace QuizEngine.QuestionGenerator
{
    public interface IQuestionGenerator
    {
        int Weight => 1;
        Task<QuestionDto?> GenerateAsync(RoomDifficultyDto difficulty);
    }
}