using osuscorefetcher.ApiClasses;
using osuscorefetcher.ScoreCalc;
using osuscorefetcher.ConfigHandler;

namespace osuscorefetcher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Config config = ConfigIO.GetConfig();
            Console.WriteLine("Testing...");
            await ApiService.SetToken(config.ApiId, config.ApiSecret);
            Console.WriteLine("Got token!");
            ScoresResponse latestScores = await ApiService.GetScores();
            for (int i = 0; i<latestScores.Scores.Length; i++)
            {
                ScoreCalculator scoreCalc = new ScoreCalculator();
                if (latestScores.Scores[i].PP == null)
                {
                    Console.WriteLine($"Score with ID {latestScores.Scores[i].Id} doesn't have a PP value attached to it. Calculating...");
                    double pp = await scoreCalc.CalculateScorePP(latestScores.Scores[i]);
                    Console.WriteLine($"Score with ID {latestScores.Scores[i].Id} has a PP value of {pp}.");
                }
            }
        }
    }
}
