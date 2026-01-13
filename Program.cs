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

            while (true)
            {
                ScoresResponse latestScores = await ApiService.GetScoresAsync(cursorString, "null");
                if (latestScores.Scores.Length == 0) break;
                Score[] rankedScores = latestScores.Scores.Where(s => s.PP != null).ToArray();
                Score[] unrankedScores = latestScores.Scores.Where(s => s.PP == null).ToArray();

                rankedScoreCount += rankedScores.Length;
                unrankedScoreCount += unrankedScores.Length;

                Task unrankedTask = service.ProcessUnrankedScoresAsync(unrankedScores);

                List<Task> rankedTasks = new List<Task>();
                int chunkCount = Math.Min(4, rankedScores.Length);
                int chunkSize = (int)Math.Ceiling((double)rankedScores.Length / chunkCount); ;

                for (int i = 0; i < chunkCount; i++)
                {
                    int start = i * chunkSize;
                    int end = (i == chunkCount - 1) ? rankedScores.Length : start + chunkSize;
                    rankedTasks.Add(service.ProcessRankedScoresAsync(rankedScores.Skip(start).Take(end - start).ToArray()));
                }

                await Task.WhenAll(new[] { unrankedTask }.Concat(rankedTasks));
                
                cursorString = latestScores.Cursor;
                config.Cursor = cursorString;
                string configJSON = JsonConvert.SerializeObject(config, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                ConfigIO.SetConfig(configJSON);

                await Task.Delay(1000);
            }

            Console.WriteLine($"Finished fetching {rankedScoreCount + unrankedScoreCount} scores");
        }
    }
}
