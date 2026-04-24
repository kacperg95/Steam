namespace Repository.Entites
{
    public class Game
    {
        public int GameId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ShortDescriptionCensored { get; set; }
        public string? HeaderImageUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        public int TotalReviews { get; set; }
        public double ReviewScore { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool LastUpdateSuccessful { get; set; } = true;

        // Relations
        public List<Achievement> Achievements { get; set; } = [];
        public List<Review> Reviews { get; set; } = [];
        public List<Genre> Genres { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
        public List<Screenshot> Screenshots { get; set; } = [];
        public List<Movie> Movies { get; set; } = [];
        public List<Publisher> Publishers { get; set; } = [];
        public List<Developer> Developers { get; set; } = [];
        public List<Tag> Tags { get; set; } = [];
    }
}
