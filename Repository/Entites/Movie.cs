namespace Repository.Entites
{
    public class Movie
    {
        public long Id { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public int GameId { get; set; }
        public Game Game { get; set; } = new();
    }
}
