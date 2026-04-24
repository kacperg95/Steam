namespace DTO.SteamApis
{
    public class FullGameDetailsDto
    {
        public int Id { get; set; }
        public int? AdditionalId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string HeaderImageUrl { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        public int TotalReviews { get; set; }
        public double ReviewScore { get; set; }

        public List<string> Publishers { get; set; } = [];
        public List<string> Developers { get; set; } = [];

        public List<FullGameDetailsCategoryOrGenreDto> Categories { get; set; } = [];
        public List<FullGameDetailsCategoryOrGenreDto> Genres { get; set; } = [];

        public List<FullGameMovieOrScreenshotDto> Movies { get; set; } = [];
        public List<FullGameMovieOrScreenshotDto> Screenshots { get; set; } = [];
        public List<string> Tags { get; set; } = [];

        public List<FullGameAchievementDto> Achievements { get; set; } = [];
        public List<FullGameReviewDto> Reviews { get; set; } = [];

        public class FullGameDetailsCategoryOrGenreDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class FullGameMovieOrScreenshotDto
        {
            public long Id { get; set; }
            public string Url { get; set; } = string.Empty;
        }

        public class FullGameAchievementDto
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string IconUrl { get; set; } = string.Empty;
            public double PercentAchieved { get; set; }
        }
        public class FullGameReviewDto
        {
            public long Id { get; set; }
            public string Author { get; set; } = string.Empty;
            public bool VotedUp { get; set; }
            public int Playtime { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}