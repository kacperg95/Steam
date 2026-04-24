namespace DTO.Quiz
{
    public class RoomDto
    {

        public string RoomCode { get; set; } = string.Empty;
        public string Connectionid { get; set; } = string.Empty;
        public List<PlayerDto> Players { get; set; } = [];
        public PlayerDto? RoomOwner => Players.FirstOrDefault();
        public RoomStateDto State { get; set; }
        public RoomDifficultyDto Difficulty { get; set; }
        public int WinScore { get; set; } = 10;
        public QuestionDto? CurrentQuestion { get; set; }
    }

    public enum RoomStateDto
    {
        Lobby,
        InGame,
        Finished
    }

    public enum RoomDifficultyDto
    {
        Tutorial = 200_000,
        Easy = 100_000,
        Medium = 50_000,
        Hard = 20_000,
        Impossible = 10_000,
        Wizard = 5_000
    }
}
