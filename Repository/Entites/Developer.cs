namespace Repository.Entites
{
    public class Developer
    {
        public int DeveloperId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = [];

    }
}
