namespace Repository.Entites
{
    public class Publisher
    {
        public int PublisherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = [];
    }
}
