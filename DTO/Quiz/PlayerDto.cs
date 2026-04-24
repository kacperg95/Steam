namespace DTO.Quiz
{
    public class PlayerDto(string connectionId, string name)
    {
        public string ConnectionId { get; set; } = connectionId;
        public string Name { get; set; } = name;
        public int Score { get; set; } = 0;
        public int AnswerIndex { get; set; } = -1;
        public DateTime? AnswerTime { get; set; } 
        public bool? IsAnswerCorrect { get; set; } = null;
        public bool? AnsweredFirst { get; set; } = null;    
    }
}
