using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    internal class Rank
    {
        [JsonProperty("global")]
        public int? Global { get; set; }
        [JsonProperty("country")]
        public int? Country { get; set; }
    }
}
