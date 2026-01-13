using Newtonsoft.Json;

namespace osuscorefetcher.ConfigHandler
{
    internal class Config
    {
        [JsonProperty("api_id")]
        public int ApiId { get; set; }
        [JsonProperty("api_secret")]
        public string ApiSecret { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("cursor_string")]
        public string Cursor { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
        [JsonProperty("db_host")]
        public string DbHost { get; set; }
        [JsonProperty("db_username")]
        public string DbUsername { get; set; }
        [JsonProperty("db_password")]
        public string DbPassword { get; set; }
        [JsonProperty("db_name")]
        public string DbName { get; set; }
    }
}
