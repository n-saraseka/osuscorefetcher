using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    internal class UserRulesetStatistics
    {
        [JsonProperty("global_rank")]
        public int? GlobalRank { get; set; }
        [JsonProperty("pp")]
        public int PP { get; set; } = 0;
    }
}
