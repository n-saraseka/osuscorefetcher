using Newtonsoft.Json;

namespace osuscorefetcher.ApiClasses
{
    public class ScoresResponse
    {
        [JsonProperty("scores")]
        public Score[] Scores { get; set; }
        [JsonProperty("cursor_string")]
        public string Cursor { get; set; }
    }
}
