using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace SteamApi
{
    public class SteamScrapper(IHttpClientFactory _httpClientFactory)
    {
        private static readonly Regex ReviewCountRegex = new(@"([\d,]+) user reviews", RegexOptions.Compiled);

        public async Task<Dictionary<int, int>> GetSteamIds(int start, bool topsellers, int count = 100)
        {
            var url = $"search/results/?query&start={start}&count={count}&category1=998";
            if (topsellers)
                url += "&filter=topsellers";

            var client = _httpClientFactory.CreateClient("SteamStore");
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var results = new Dictionary<int, int>();

            var gameNodes = doc.DocumentNode.SelectNodes("//a[@data-ds-appid]");
            if (gameNodes == null)
                return results;

            foreach (var node in gameNodes)
            {
                var appIdStr = node.GetAttributeValue("data-ds-appid", null!);
                if (!int.TryParse(appIdStr, out var appId))
                    continue;

                var reviewSpan = node.SelectSingleNode(".//span[contains(@class,'search_review_summary')]");
                var tooltip = reviewSpan?.GetAttributeValue("data-tooltip-html", null!);

                int totalReviews = 0;
                if (tooltip != null)
                {
                    var match = ReviewCountRegex.Match(tooltip);
                    if (match.Success)
                        int.TryParse(match.Groups[1].Value.Replace(",", ""), out totalReviews);
                }

                results.TryAdd(appId, totalReviews);
            }

            return results;
        }

        public async Task<List<string>> GetTags(int appId)
        {
            var url = $"apphover/{appId}?l=english";
            var client = _httpClientFactory.CreateClient("SteamStore");
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tags = new List<string>();
            var tagRow = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'hover_tag_row')]");
            if (tagRow == null)
                return tags;

            var tagNodes = tagRow.SelectNodes(".//div[contains(@class,'app_tag')]");
            if (tagNodes == null)
                return tags;

            foreach (var node in tagNodes)
            {
                var text = node.InnerText?.Trim();
                if (!string.IsNullOrEmpty(text))
                    tags.Add(text);
            }

            return tags;
        }
    }
}
