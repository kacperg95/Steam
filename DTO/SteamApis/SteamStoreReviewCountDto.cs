using System.Text.Json.Serialization;

namespace DTO.SteamApis
{
    public class SteamStoreReviewCountDto
    {
        [JsonPropertyName("total_positive")]
        public int TotalPositive { get; set; }

        [JsonPropertyName("total_negative")]
        public int TotalNegative { get; set; }

        [JsonPropertyName("total_reviews")]
        public int TotalReviews { get; set; }
    }

    public class SteamStoreReviewCountResponseDto
    {
        [JsonPropertyName("query_summary")]
        public SteamStoreReviewCountDto QuerySummary { get; set; } = new();
    }
}
