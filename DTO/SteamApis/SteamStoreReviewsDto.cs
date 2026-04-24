using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DTO.SteamApis
{
    public class SteamStoreReviewsDto
    {
        public List<Review> Reviews { get; set; } = [];

        public class Review
        {
            public long RecommendationId { get; set; }
            public Author Author { get; set; } = new();

            [JsonPropertyName("review")]
            public string Content { get; set; } = string.Empty;

            [JsonPropertyName("voted_up")]
            public bool VotedUp { get; set; }
        }

        public class Author
        {
            [JsonPropertyName("personaname")]
            public string PersonaName { get; set; } = string.Empty;

            [JsonPropertyName("playtime_at_review")]
            public int Playtime { get; set; }
        }
    }
}
