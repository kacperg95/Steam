namespace Repository.Entites
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = [];

    }
}
