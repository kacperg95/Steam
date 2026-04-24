namespace Repository.Entites
{
    public class Screenshot
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int GameId { get; set; }
        public Game Game { get; set; } = new();
    }
}
