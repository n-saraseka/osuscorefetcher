using osuscorefetcher.ApiClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Entities
{
    internal class DbScore
    {
        /// <summary>
        /// Get score from the DB
        /// </summary>
        /// <param name="id">Score ID</param>
        /// <returns>Populated Score object (or null in case it doesn't exist in the DB)</returns>
        public Score? GetScore(ulong id)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                Score? score = db.Scores.FirstOrDefault(s => s.Id == id);
                return score;
            }
        }

        /// <summary>
        /// Insert Score data into the DB
        /// </summary>
        /// <param name="score">Populated Score object</param>
        public void InsertScore(Score score)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Scores.Add(score);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Insert data for multiple Scores
        /// </summary>
        /// <param name="scores">An IEnumerable containing populated Score objects</param>
        public void InsertScores(IEnumerable<Score> scores)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Scores.AddRange(scores);
                db.SaveChanges();
            }
        }
    }
}
