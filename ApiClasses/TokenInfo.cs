using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    internal class TokenInfo
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }
}
