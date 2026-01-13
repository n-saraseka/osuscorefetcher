using osu.Game.Graphics.UserInterface;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;
using osuscorefetcher.ScoreCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher
{
    internal class ScoreFetcherService
    {
        private static readonly Config Config = ConfigIO.GetConfig();
        private static readonly ScoreCalculator ScoreCalc = new ScoreCalculator();

        /// <summary>
        /// Process data from unranked scores, including PP calculation. Calculates highest PP scores for each mode
        /// </summary>
        /// <param name="scores">Unranked scores to process</param>
        /// <returns></returns>
        public async Task ProcessUnrankedScoresAsync(Score[] scores)
        {
            DateTime start = scores[0].Date;
            DateTime end = scores[scores.Length - 1].Date;

            int scoresCounter = scores.Length;

            for (int i = 0; i < scores.Length; i++)
            {
                int currentMode = (int)scores[i].Mode;
                scores[i].PP = await ScoreCalc.CalculateScorePPAsync(scores[i]);
                // ratelimits duh
                if (i < scores.Length - 1)
                    await Task.Delay(1000);
            }

            Console.WriteLine($"Fetched {scoresCounter} unranked scores between {start} and {end}.");
            foreach (Mode gameplayMode in Enum.GetValues(typeof(Mode)))
            {
                Score[] scoresForThisMode = scores.Where(s => s.Mode == gameplayMode).ToArray();
                if (scoresForThisMode.Length > 0)
                {
                    Score highestPPScore = scoresForThisMode.Aggregate((s1, s2) => s1.PP > s2.PP ? s1 : s2);
                    Console.WriteLine($"Highest PP score for mode {gameplayMode}: {highestPPScore.PP}pp (ID: {highestPPScore.Id})");
                }
                else Console.WriteLine($"No scores for mode {gameplayMode} in this time interval");
            }
        }
        /// <summary>
        /// Process data from ranked scores, calculating highest PP scores for each mode
        /// </summary>
        /// <param name="scores">Ranked scores to process</param>
        /// <returns></returns>
        public async Task ProcessRankedScoresAsync(Score[] scores)
        {
            DateTime start = scores[0].Date;
            DateTime end = scores[scores.Length - 1].Date;

            int scoresCounter = scores.Length;

            Console.WriteLine($"Fetched {scoresCounter} ranked scores between {start} and {end}.");
            foreach (Mode gameplayMode in Enum.GetValues(typeof(Mode)))
            {
                Score[] scoresForThisMode = scores.Where(s => s.Mode == gameplayMode).ToArray();
                if (scoresForThisMode.Length > 0)
                {
                    Score highestPPScore = scoresForThisMode.Aggregate((s1, s2) => s1.PP > s2.PP ? s1 : s2);
                    Console.WriteLine($"Highest PP score for mode {gameplayMode}: {highestPPScore.PP}pp (ID: {highestPPScore.Id})");
                }
                else Console.WriteLine($"No scores for mode {gameplayMode} in this time interval");
            }
        }
    }
}
