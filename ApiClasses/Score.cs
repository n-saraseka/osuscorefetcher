using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.API;


namespace osuscorefetcher.ApiClasses
{
    internal class Score
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("ended_at")]
        public DateTime Date {  get; set; }
        [JsonProperty("ruleset_id")]
        public Mode Mode { get; set; }
        [JsonProperty("beatmap_id")]
        public int BeatmapId { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("user")]
        public User? User { get; set; }
        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Grade Grade { get; set; }
        [JsonProperty("mods")]
        public APIMod[] Mods { get; set; } = Array.Empty<APIMod>();
        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }
        [JsonProperty("max_combo")]
        public int Combo { get; set; }
        [JsonProperty("statistics")]
        public Statistics Statistics { get; set; } = null!;
        [JsonProperty("total_score")]
        public uint TotalScore { get; set; }
        [JsonProperty("pp")]
        public double? PP { get; set; }
    }
}
