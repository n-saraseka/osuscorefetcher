using System.Text;
using Newtonsoft.Json;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;

namespace osuscorefetcher
{
    internal class ApiService
    {
        private static Config Config = ConfigIO.GetConfig();
        public const string TokenUrl = "https://osu.ppy.sh/oauth/token";
        public const string ApiUrl = "https://osu.ppy.sh/api/v2";
        public const int ApiVersion = 20220705;
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Set fresh token data for API access
        /// </summary>
        /// <param name="apiId">OAuth API client ID</param>
        /// <param name="apiSecret">OAuth API client secret</param>
        /// <returns></returns>
        public static async Task SetTokenAsync(int apiId, string apiSecret, long currentTime)
        {
            long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("client_id", apiId.ToString());
            data.Add("client_secret", apiSecret);
            data.Add("grant_type", "client_credentials");
            data.Add("scope", "public");

            string dataJSON = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            requestMessage.Content = new StringContent(dataJSON, Encoding.UTF8, "application/json");

            // getting the token
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            // writing new token data
            TokenInfo tokenData = JsonConvert.DeserializeObject<TokenInfo>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            tokenData.ExpiresIn += currentTime;
            Config.AccessToken = tokenData.AccessToken;
            Config.ExpiresIn = tokenData.ExpiresIn;
            string configJSON = JsonConvert.SerializeObject(Config);
            Config = ConfigIO.SetConfig(configJSON);
        }
        
        /// <summary>
        /// Get scores from the API firehose
        /// </summary>
        /// <param name="cursor">Cursor string (used to fetch new scores since last call)</param>
        /// <param name="ruleset">Ruleset name (osu, mania, taiko, fruits)</param>
        /// <returns>Populated ScoresResponse object with the cursor string and array of Scores</returns>
        public static async Task<ScoresResponse> GetScoresAsync(string cursor = "null", string ruleset = "null")
        {
            await CheckIfTokenIsValidAsync();

                Dictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                {"ruleset", ruleset},
                {"cursor_string", cursor}
            };

            if (ruleset == "null")
            {
                queryParameters.Remove("ruleset");
            }

            string queryString = string.Join("&", queryParameters.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/scores?{queryString}");

            // assemble headers
            requestMessage.Headers.Add("Authorization", "Bearer " + Config.AccessToken);
            requestMessage.Headers.Add("x-api-version", ApiVersion.ToString());

            // getting scores
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            ScoresResponse scores = JsonConvert.DeserializeObject<ScoresResponse>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return scores;
        }

        /// <summary>
        /// Get API Beatmap data from their IDs
        /// </summary>
        /// <param name="ids">List containing beatmap IDs</param>
        /// <returns>List with populated APIBeatmap objects</returns>
        public static async Task<List<APIBeatmap>> GetBeatmapsAsync(List<int> ids)
        {
            await CheckIfTokenIsValidAsync();

            int count = ids.Count;
            if (count == 0) throw new ArgumentException("No beatmap IDs to process");
            if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

            string queryString = string.Join("&", ids.Select(b => $"ids[]={b}"));
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/beatmaps?{queryString}");

            // assemble headers
            requestMessage.Headers.Add("Authorization", "Bearer " + Config.AccessToken);
            requestMessage.Headers.Add("x-api-version", ApiVersion.ToString());

            // parse beatmaps
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            List<APIBeatmap> beatmaps = JsonConvert.DeserializeObject<List<APIBeatmap>>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return beatmaps;
        }

        /// <summary>
        /// Get API User data from their IDs
        /// </summary>
        /// <param name="ids">List containing user IDs</param>
        /// <returns>List with populated User objects</returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<List<User>> GetUsersAsync(List<int> ids)
        {
            await CheckIfTokenIsValidAsync();

            int count = ids.Count;
            if (count == 0) throw new ArgumentException("No user IDs to process");
            if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

            string queryString = string.Join("&", ids.Select(u => $"ids[]={u}"));
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/users?{queryString}");

            // assemble headers
            requestMessage.Headers.Add("Authorization", "Bearer " + Config.AccessToken);
            requestMessage.Headers.Add("x-api-version", ApiVersion.ToString());

            // parse beatmaps
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            List<User> users = JsonConvert.DeserializeObject<List<User>>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return users;
        }

        /// <summary>
        /// Check if token has expired
        /// </summary>
        /// <returns></returns>
        public static async Task CheckIfTokenIsValidAsync()
        {
            long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Config.AccessToken == null || seconds > Config?.ExpiresIn) await SetTokenAsync(Config.ApiId, Config.ApiSecret, seconds);
        }
    }
}
