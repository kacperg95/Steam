namespace Repository.Entites
{
    public class Review
    {
        public long Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public int Playtime { get; set; }
        public DateTime CreatedAt { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; } = new();
    }
}
