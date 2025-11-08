using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Online.API;
using osuscorefetcher.ApiClasses;
using System.Reflection;


namespace osuscorefetcher.ScoreCalc
{
    internal class ScoreCalculator
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public async Task<double> CalculateScorePP(ApiClasses.Score score)
        {
            // preparing necessary data
            Ruleset ruleset = GetRulesetFromScore(score);
            ScoreInfo scoreInfo = GetScoreInfo(score, ruleset);
            IBeatmap beatmap = await GetScoreBeatmap(score);
            FlatWorkingBeatmap flatWorkingBeatmap = new FlatWorkingBeatmap(beatmap);
            // because PerformanceCalculator takes the difficulty settings from ScoreInfo
            scoreInfo.BeatmapInfo = flatWorkingBeatmap.BeatmapInfo; 

            // diffcalc
            DifficultyAttributes difficultyAttributes = ruleset.CreateDifficultyCalculator(flatWorkingBeatmap).Calculate(scoreInfo.Mods);
            PerformanceCalculator performanceCalculator = ruleset.CreatePerformanceCalculator();
            PerformanceAttributes performanceAttributes = await performanceCalculator.CalculateAsync(scoreInfo, difficultyAttributes, default);
            score.PP = performanceAttributes.Total;

            return (double)score.PP;
        }
        public ScoreInfo GetScoreInfo(ApiClasses.Score score, Ruleset ruleset)
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

            return soloScoreInfo.ToScoreInfo(ModsArray);
        }
        public async Task<Beatmap> GetScoreBeatmap(ApiClasses.Score score) {
            using Stream stream = await httpClient.GetStreamAsync($"https://osu.ppy.sh/osu/{score.BeatmapId}");
            using LineBufferedReader reader = new LineBufferedReader(stream);
            Beatmap beatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
            return beatmap;
        }
        public Ruleset GetRulesetFromScore(ApiClasses.Score score) {
            switch (score.Mode)
            {
                case Mode.Osu:
                    {
                        return new OsuRuleset();
                    }
                case Mode.Taiko:
                    {
                        return new TaikoRuleset();
                    }
                case Mode.Fruits:
                    {
                        return new CatchRuleset();
                    }
                default:
                    {
                        return new ManiaRuleset();
                    }
            }
        }
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
    }
}
