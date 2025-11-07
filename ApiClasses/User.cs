using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    internal class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("country")]
        public Country Country { get; set; }
        [JsonProperty("rank")]
        public Rank? Rank { get; set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
    }
}
