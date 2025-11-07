using System.Text;
using Newtonsoft.Json;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;

namespace osuscorefetcher
{
    internal class ApiService()
    {
        private static Config Config { get; set; }
        public const string TokenUrl = "https://osu.ppy.sh/oauth/token";
        public const string ApiUrl = "https://osu.ppy.sh/api/v2";
        public static async Task SetToken(int apiId, string apiSecret)
        {

            // getting current token data
            Config = ConfigIO.GetConfig();

            long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (Config.AccessToken == null || seconds > Config?.ExpiresIn)
            {
                // need to get a new token
                HttpClient client = new HttpClient();

                // assemble params
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("client_id", apiId.ToString());
                data.Add("client_secret", apiSecret);
                data.Add("grant_type", "client_credentials");
                data.Add("scope", "public");

                string dataJSON = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
                requestMessage.Content = new StringContent(dataJSON, Encoding.UTF8, "application/json");

                // getting the token
                HttpResponseMessage response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();

                client.Dispose();

                // writing new token data
                TokenInfo tokenData = JsonConvert.DeserializeObject<TokenInfo>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                tokenData.ExpiresIn += seconds;
                Config.AccessToken = tokenData.AccessToken;
                Config.ExpiresIn = tokenData.ExpiresIn;
                string configJSON = JsonConvert.SerializeObject(Config, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Config = ConfigIO.SetConfig(configJSON);
            }
        }
        public static async Task<ScoresResponse> GetScores(string cursor = "null", string ruleset = "null", int apiVersion = 20220705)
        {
            HttpClient client = new HttpClient();

            Dictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                {"ruleset", ruleset},
                {"cursor_string", cursor}
            };

            string queryString = string.Join("&", queryParameters.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, ApiUrl + "/scores");

            // assemble headers
            requestMessage.Headers.Add("Authorization", "Bearer " + Config.AccessToken);
            requestMessage.Headers.Add("x-api-version", apiVersion.ToString());

            // getting scores
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            client.Dispose();

            ScoresResponse scores = JsonConvert.DeserializeObject<ScoresResponse>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return scores;
        }
    }
}
