using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    internal class Country
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
