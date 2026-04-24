using System.Text.Json.Serialization;

namespace DTO.SteamApis
{
    public class SteamPoweredAchievementPercentagesDto
    {
        [JsonPropertyName("achievementpercentages")]
        public AchievementContainer AchievementPercentages { get; set; } = new();

        public class AchievementContainer
        {
            public List<AchievementItem> Achievements { get; set; } = [];
        }

        public class AchievementItem
        {
            public string Name { get; set; } = string.Empty;
            public double Percent { get; set; }
        }
    }
}
