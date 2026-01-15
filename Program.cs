using Newtonsoft.Json;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;
using osuscorefetcher.ScoreCalc;

namespace osuscorefetcher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Config config = ConfigIO.GetConfig();
            string cursorString = config.Cursor;
            Console.WriteLine($"Started fetching scores at {DateTime.Now}");
            int rankedScoreCount = 0;
            int unrankedScoreCount = 0;

            ScoreFetcherService service = new ScoreFetcherService();

            ScoresResponse latestScores = await ApiService.GetScoresAsync(cursorString, "null");
            Score[] rankedScores = latestScores.Scores.Where(s => s.PP != null).ToArray();
            Score[] unrankedScores = latestScores.Scores.Where(s => s.PP == null).ToArray();

            rankedScoreCount += rankedScores.Length;
            unrankedScoreCount += unrankedScores.Length;

            Task<List<APIBeatmap>> beatmapsTask = service.ProcessBeatmapsAsync(latestScores.Scores);
            Task<List<User>> usersTask = service.ProcessUsersAsync(latestScores.Scores);
            await Task.WhenAll(beatmapsTask, usersTask);

            Task unrankedTask = service.ProcessUnrankedScoresAsync(unrankedScores);
            Task rankedTask = service.ProcessRankedScoresAsync(rankedScores);

            await Task.WhenAll(unrankedTask, rankedTask);

            Console.WriteLine($"Added {rankedScoreCount + unrankedScoreCount} Scores to the DB");

            cursorString = latestScores.Cursor;
            config.Cursor = cursorString;
            string configJSON = JsonConvert.SerializeObject(config, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            ConfigIO.SetConfig(configJSON);

            Console.WriteLine($"Finished fetching {rankedScoreCount + unrankedScoreCount} scores");
        }
    }
}
