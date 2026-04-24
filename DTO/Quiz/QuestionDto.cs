namespace DTO.Quiz
{
    public class QuestionDto
    {
        public string Content { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? MediaUrl { get; set; }
        public int TimeToAnswer { get; set; }
        public List<QuestionAnswerDto> Answers { get; set; } = [];
    }

    public record QuestionAnswerDto
    {
        public string Content { get; set; } = string.Empty;
        public string? PostAnswerContent { get; set; }
        public string? MediaUrl { get; set; }
        public bool IsValid { get; set; }
    }
}