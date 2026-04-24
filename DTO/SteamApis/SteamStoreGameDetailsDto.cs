using System.Text.Json.Serialization;

namespace DTO.SteamApis
{
    public class SteamStoreGameDetailsDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamStoreGameDto Data { get; set; } = new();

        public class SteamStoreGameDto
        {
            [JsonPropertyName("steam_appid")]
            public int AppId { get; set; }
            public string Name { get; set; } = string.Empty;
            [JsonPropertyName("short_description")]
            public string ShortDescription { get; set; } = string.Empty;

            [JsonPropertyName("header_image")]
            public string HeaderImageUrl { get; set; } = string.Empty;

            [JsonPropertyName("release_date")]
            public SteamStoreReleaseDateDto ReleaseDate { get; set; } = new();

            public List<string> Developers { get; set; } = [];
            public List<string> Publishers { get; set; } = [];
            public List<SteamStoreCategoryOrGenreDto> Categories { get; set; } = [];
            public List<SteamStoreCategoryOrGenreDto> Genres { get; set; } = [];

            public List<SteamStoreMovieDto> Movies { get; set; } = [];
            public List<SteamStoreScreenshotDto> Screenshots { get; set; } = [];
        }

        public class SteamStoreCategoryOrGenreDto
        {
            public int Id { get; set; }
            public required string Description { get; set; }
        }

        public class SteamStoreMovieDto
        {
            public long Id { get; set; }
            [JsonPropertyName("hls_h264")]
            public string Url { get; set; } = string.Empty;
        }

        public class SteamStoreScreenshotDto
        {
            public long Id { get; set; }
            [JsonPropertyName("path_full")]
            public string Url { get; set; } = string.Empty;
        }

        public class SteamStoreReleaseDateDto
        {
            [JsonPropertyName("date")]
            public string Date { get; set; } = string.Empty;
        }
    }


    

}
