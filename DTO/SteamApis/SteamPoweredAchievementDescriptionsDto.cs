using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DTO.SteamApis
{
    public class SteamPoweredAchievementDescriptionsDto
    {
        public GameData Game { get; set; } = new();

        public class GameData
        {
            public GameStats AvailableGameStats { get; set; } = new();
        }

        public class GameStats
        {
            public List<AchievementDetail> Achievements { get; set; } = [];
        }

        public class AchievementDetail
        {
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("icon")]
            public string IconUrl { get; set; } = string.Empty;
        }
    }
}
