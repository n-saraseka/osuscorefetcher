using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osuscorefetcher.ApiClasses;
using System.Reflection;


namespace osuscorefetcher.ScoreCalc
{
    internal class ScoreCalculator
    {
        private static readonly HttpClient httpClient = new HttpClient();
        /// <summary>
        /// Calculate PP of a given Score
        /// </summary>
        /// <param name="score">Score data from the API</param>
        /// <returns>Score's PP</returns>
        public async Task<float?> CalculateScorePPAsync(ApiClasses.Score score)
        {
            // preparing necessary data
            Ruleset ruleset = GetRulesetFromScore(score);
            IBeatmap beatmap = new Beatmap();
            try
            {
                beatmap = await GetScoreBeatmapAsync(score);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't get the beatmap for score {score.Id}. Beatmap ID: {score.BeatmapId}.");
                Console.WriteLine($"Failed with the following exception: {ex.Message}");
                return null;
            }
            ScoreInfo scoreInfo = GetScoreInfo(score, beatmap, ruleset);
            FlatWorkingBeatmap flatWorkingBeatmap = new FlatWorkingBeatmap(beatmap);

            // diffcalc
            DifficultyAttributes difficultyAttributes = ruleset.CreateDifficultyCalculator(flatWorkingBeatmap).Calculate(scoreInfo.Mods);
            PerformanceCalculator performanceCalculator = ruleset.CreatePerformanceCalculator();
            PerformanceAttributes performanceAttributes = await performanceCalculator.CalculateAsync(scoreInfo, difficultyAttributes, default);

            return (float)performanceAttributes.Total;
        }
        /// <summary>
        /// Prepare ScoreInfo object for use in calculating difficulty and performance attributes
        /// </summary>
        /// <param name="score">Score data from the API</param>
        /// <param name="beatmap">Beatmap data for this score</param>
        /// <param name="ruleset">This score's Ruleset</param>
        /// <returns>The populated ScoreInfo</returns>
        public ScoreInfo GetScoreInfo(ApiClasses.Score score, IBeatmap beatmap, Ruleset ruleset)
        {
            Dictionary<HitResult, int> ScoreStatistics = ScoreStatisticsToDict(score.Statistics);
            Dictionary<HitResult, int> MaximumStatistics = ScoreStatisticsToDict(score.MaximumStatistics);

            SoloScoreInfo soloScoreInfo = new SoloScoreInfo
            {
                BeatmapID = score.BeatmapId,
                RulesetID = (int)score.Mode,
                TotalScore = score.TotalScore,
                LegacyTotalScore = score.LegacyTotalScore,
                LegacyScoreId = score.LegacyScoreId,
                Accuracy = score.Accuracy,
                UserID = score.UserId,
                MaxCombo = score.Combo,
                Rank = (ScoreRank)score.Grade,
                EndedAt = score.Date,
                Mods = score.Mods,
                Statistics = ScoreStatistics,
                MaximumStatistics = MaximumStatistics
            };

            List<Mod> Mods = new List<Mod>();
            foreach (APIMod apiMod in score.Mods)
            {
                Mod mod = apiMod.ToMod(ruleset);
                Mods.Add(mod);
            }
            Mod[] ModsArray = Mods.ToArray();

            return soloScoreInfo.ToScoreInfo(ModsArray, beatmap.BeatmapInfo);
        }
        /// <summary>
        /// Download a map from the API and decode it into a Beatmap object
        /// </summary>
        /// <param name="score">Score object to parse the beatmap ID from</param>
        /// <returns>Parsed Beatmap object</returns>
        public async Task<Beatmap> GetScoreBeatmapAsync(ApiClasses.Score score) {
            using Stream stream = await httpClient.GetStreamAsync($"https://osu.ppy.sh/osu/{score.BeatmapId}");
            using LineBufferedReader reader = new LineBufferedReader(stream);
            Beatmap beatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
            return beatmap;
        }
        /// <summary>
        /// Parses the Ruleset from given API Score data
        /// </summary>
        /// <param name="score">Score object to parse the ruleset from</param>
        /// <returns>Corresponding Ruleset object</returns>
        public Ruleset GetRulesetFromScore(ApiClasses.Score score) {
            switch (score.Mode)
            {
                case Mode.Osu:
                        return new OsuRuleset();
                case Mode.Taiko:
                        return new TaikoRuleset();
                case Mode.Fruits:
                        return new CatchRuleset();
                default:
                        return new ManiaRuleset();
            }
        }
        /// <summary>
        /// Creates a dictionary of statistics for each HitResult from API Statistics data
        /// </summary>
        /// <param name="stats">Hit statistics</param>
        /// <returns>Populated dictionary of statistics for each HitResult</returns>
        public Dictionary<HitResult, int> ScoreStatisticsToDict(Statistics stats)
        {
            Dictionary<HitResult, int> ScoreStatistics = new Dictionary<HitResult, int>();

            foreach (var property in typeof(Statistics).GetProperties())
            {
                var hitResultAttribute = property.GetCustomAttribute<HitResultAttribute>();
                if (hitResultAttribute != null)
                {
                    int? value = (int?)property.GetValue(stats);
                    if (value != null)
                    {
                        ScoreStatistics[hitResultAttribute.HitResult] = (int)value;
                    }
                }
            }
            return ScoreStatistics;
        }
        /// <summary>
        /// Parse score's global rank on the beatmap
        /// </summary>
        /// <param name="score">Score to process the rank from</param>
        /// <returns>Global rank (or null in case the map is neither Loved or Ranked)</returns>
        public async Task<int?> ParseScoreRank(ApiClasses.Score score) {
            string scoreInfoUrl = $"https://osu.ppy.sh/scores/{score.Id}";
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(scoreInfoUrl);
            string rankSpan = "div.score-player__rank--value span";
            string? rankValue = document.QuerySelector(rankSpan)?.TextContent ?? null;
            int? rankValueParsed = (rankValue != null) ? int.Parse(rankValue) : null;
            return rankValueParsed;
        }
    }
}
