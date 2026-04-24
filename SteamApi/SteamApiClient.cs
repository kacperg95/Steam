using DTO.SteamApis;
using Microsoft.Extensions.Options;
using Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace SteamApi
{
    public class SteamApiClient(IHttpClientFactory _httpClientFactory, SteamScrapper _steamScrapper, Mapper _mapper, IOptions<ApiOptions> options)
    {
        private readonly ApiOptions _options = options.Value;
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public async Task<Dictionary<int, int>> GetGameIds(int start, bool topsellers, int count = 100) => await _steamScrapper.GetSteamIds(start, topsellers, count);

        public async Task<FullGameDetailsDto?> GetGame(int appId)
        {
            var steamStoreClient = _httpClientFactory.CreateClient("SteamStore");
            var steamApiClient = _httpClientFactory.CreateClient("SteamApi");
            int? additionalGameId = null;

            var storeResponse = await steamStoreClient.GetFromJsonAsync<Dictionary<string, SteamStoreGameDetailsDto>>($"api/appdetails?appids={appId}&l=en&key={_options.SteamApiKey}");
            if (storeResponse == null || !storeResponse.TryGetValue(appId.ToString(), out var storeData) || !storeData.Success)
                return null;

            if (appId != storeData.Data.AppId)
            {
                additionalGameId = appId;
                appId = storeData.Data.AppId;
                storeResponse = await steamStoreClient.GetFromJsonAsync<Dictionary<string, SteamStoreGameDetailsDto>>($"api/appdetails?appids={appId}&l=en&key={_options.SteamApiKey}");
                if (storeResponse == null || !storeResponse.TryGetValue(appId.ToString(), out storeData) || !storeData.Success)
                    return null;
            }

            var achivementsTask = steamApiClient.GetAsync($"ISteamUserStats/GetSchemaForGame/v2/?appid={appId}&l=en&key={_options.SteamApiKey}");
            var percentagesTask = steamApiClient.GetAsync($"ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?gameid={appId}&l=en&key={_options.SteamApiKey}");
            var reviewCountTask = steamStoreClient.GetAsync($"appreviews/{appId}?json=1&language=all&num_per_page=0&purchase_type=all&key={_options.SteamApiKey}");
            var reviewsTask = steamStoreClient.GetAsync($"appreviews/{appId}?json=1&filter=all&language=english&num_per_page=10&purchase_type=all&key={_options.SteamApiKey}");
            var tagsTask = _steamScrapper.GetTags(appId);
            await Task.WhenAll(achivementsTask, percentagesTask, reviewCountTask, reviewsTask);

            var errors = new List<Exception>();
            var achievements = await TryDeserializeResponse<SteamPoweredAchievementDescriptionsDto>(achivementsTask.Result, errors);
            var percentages = await TryDeserializeResponse<SteamPoweredAchievementPercentagesDto>(percentagesTask.Result, errors);
            var reviewCount = await TryDeserializeResponse<SteamStoreReviewCountResponseDto>(reviewCountTask.Result, errors);
            var reviews = await TryDeserializeResponse<SteamStoreReviewsDto>(reviewsTask.Result, errors);
            var tags = await tagsTask;

            if (errors.Count != 0)
                throw new AggregateException("Unable to get game data from APIs", errors);

            return _mapper.MapGame(storeData, achievements, percentages, reviewCount!, reviews!, tags);
        }

        private async Task<T?> TryDeserializeResponse<T>(HttpResponseMessage response, List<Exception> errorList) where T : class
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (ResponseIsEmpty(content)) return null;
                errorList.Add(new HttpRequestException($"API Error {response.StatusCode}: {content}"));
                return null;
            }

            try
            {
                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]")
                    return null;

                return JsonSerializer.Deserialize<T>(json, jsonOptions);
            }
            catch (JsonException ex)
            {
                errorList.Add(ex);
                return null;
            }

            bool ResponseIsEmpty(string response) => response is "{}" or "[]" or "{\"game\":{}}";
        }

    }
}
