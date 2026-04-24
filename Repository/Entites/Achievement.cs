namespace Repository.Entites
{
    public class Achievement
    {
        public long Id { get; set; }
        public string ApiName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public double GlobalUnlockPercentage { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; } = new();
    }
}
