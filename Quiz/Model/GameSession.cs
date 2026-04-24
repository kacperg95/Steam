using DTO.Quiz;

namespace Quiz.Model
{
    public class GameSession
    {
        public string Nickname { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public RoomDifficultyDto SelectedDifficulty { get; set; } = RoomDifficultyDto.Medium;
        public string? Winner { get; set; } 
    }
}
