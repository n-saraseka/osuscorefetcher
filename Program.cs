using osuscorefetcher.ApiClasses;
using osuscorefetcher.ScoreCalc;
using osuscorefetcher.ConfigHandler;

namespace osuscorefetcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Config config = ConfigIO.GetConfig();
            Console.WriteLine("Testing...");
            ApiService.SetToken(config.ApiId, config.ApiSecret).Wait();
            Console.WriteLine("Got token!");
            ScoresResponse latestScores = ApiService.GetScores("null", "osu").Result;
            for (int i = 0; i<latestScores.Scores.Length; i++)
            {
                ScoreCalculator scoreCalc = new ScoreCalculator(new HttpClient());
                if (latestScores.Scores[i].PP == null)
                {
                    Console.WriteLine($"Score with ID {latestScores.Scores[i].Id} doesn't have a PP value attached to it. Calculating...");
                    double pp = scoreCalc.CalculateScorePP(latestScores.Scores[i]).Result;
                    Console.WriteLine($"Score with ID {latestScores.Scores[i].Id} has a PP value of {pp}.");
                }
            }
        }
    }
}
