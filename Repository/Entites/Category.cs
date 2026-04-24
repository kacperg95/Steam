namespace Repository.Entites
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = [];
    }
}
