using NUnit.Framework;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;
using osuscorefetcher.DbService.Entities;
using osuscorefetcher.ScoreCalc;
using System.Linq;
using System.Threading.RateLimiting;

namespace osuscorefetcher
{
    internal class ScoreFetcherService
    {
        private static readonly Config Config = ConfigIO.GetConfig();
        private static readonly ScoreCalculator ScoreCalc = new ScoreCalculator();
        private static TokenBucketRateLimiter limiter;

        public ScoreFetcherService()
        {
            limiter = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 60,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 120,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = 1,
                AutoReplenishment = true
            });
        }

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
                using RateLimitLease lease = await limiter.AcquireAsync();
                if (lease.IsAcquired)
                    scores[i].PP = await ScoreCalc.CalculateScorePPAsync(scores[i]);
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

            using ScoreRepository scoreRepository = new();
            scoreRepository.CreateBulk(scores);
            scoreRepository.Save();
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

            using ScoreRepository scoreRepository = new();
            scoreRepository.CreateBulk(scores);
            scoreRepository.Save();
        }

        /// <summary>
        /// Process beatmap data from scores and add them to the DB
        /// </summary>
        /// <param name="scores">Array containing populated Score objects</param>
        /// <returns>List with distinct populated Beatmap objects</returns>
        public async Task<List<APIBeatmap>> ProcessBeatmapsAsync(Score[] scores)
        {
            List<int> beatmapIds = scores
                .GroupBy(s => s.BeatmapId)
                .Select(g => g.First().BeatmapId)
                .ToList();
            List<APIBeatmap> result = new List<APIBeatmap>();
            const int batchSize = 50;

            if (beatmapIds.Count > 0)
            {
                for (int i = 0; i < beatmapIds.Count; i += batchSize)
                {
                    List<int> batch = beatmapIds.Skip(i).Take(batchSize).ToList();
                    using RateLimitLease lease = await limiter.AcquireAsync();
                    if (lease.IsAcquired)
                    {
                        APIBeatmap[] beatmaps = await ApiService.GetBeatmapsAsync(batch);
                        result.AddRange(beatmaps);
                    }
                }
            }

            using BeatmapRepository beatmapRepository = new();
            List<int> existingBeatmapIds = beatmapRepository.GetAll().Select(b => b.Id).ToList();
            List<APIBeatmap> newBeatmaps = result.Where(b => !existingBeatmapIds.Contains(b.Id)).ToList();
            await Task.Run(() => {
                beatmapRepository.CreateBulk(newBeatmaps);
                beatmapRepository.Save();
            });

            Console.WriteLine($"Added {newBeatmaps.Count} Beatmaps to the DB");

            return result;
        }

        /// <summary>
        /// Process user data from scores and add them to the DB
        /// </summary>
        /// <param name="scores">Array containing populated Score objects</param>
        /// <returns>List with distinct populated User objects</returns>
        public async Task<List<User>> ProcessUsersAsync(Score[] scores)
        {
            List<int> userIds = scores
                .GroupBy(s => s.UserId)
                .Select(g => g.First().UserId)
                .ToList();
            List<User> result = new List<User>();
            const int batchSize = 50;

            if (userIds.Count > 0)
            {
                for (int i = 0; i < userIds.Count; i += batchSize)
                {
                    List<int> batch = userIds.Skip(i).Take(batchSize).ToList();
                    using RateLimitLease lease = await limiter.AcquireAsync();
                    if (lease.IsAcquired)
                    {
                        User[] users = await ApiService.GetUsersAsync(batch);
                        result.AddRange(users);
                    }
                }
            }

            using UserRepository userRepository = new();
            List<int> existingUserIds = userRepository.GetAll().Select(u => u.Id).ToList();
            List<User> newUsers = result.Where(u => !existingUserIds.Contains(u.Id)).ToList();
            await Task.Run(() => {
                userRepository.CreateBulk(newUsers);
                userRepository.Save();
            });

            Console.WriteLine($"Added {newUsers.Count} Users to the DB");

            return result;
        }
    }
}
