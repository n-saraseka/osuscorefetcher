using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using osuscorefetcher.ApiClasses;
using System.Reflection;

namespace osuscorefetcher.DbService.Entities
{
    internal class DbBeatmapBeatmapset
    {
        /// <summary>
        /// Get Beatmapset from the DB
        /// </summary>
        /// <param name="id">Beatmapset ID</param>
        /// <returns>Populated Beatmapset object (or null in case it doesn't exist in the DB)</returns>
        public Beatmapset? GetBeatmapset(int id)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                Beatmapset? beatmapset = db.Beatmapsets.FirstOrDefault(bs => bs.Id == id);
                return beatmapset;
            }
        }

        /// <summary>
        /// Insert beatmapset into the DB
        /// </summary>
        /// <param name="beatmapset">Populated Beatmapset object</param>
        public void InsertBeatmapset(Beatmapset beatmapset)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Beatmapsets.Add(beatmapset);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get a Beatmap from the DB
        /// </summary>
        /// <param name="id">Beatmap ID</param>
        /// <returns>Populated Beatmap object (or null in case it doesn't exist in the DB)</returns>
        public APIBeatmap? GetBeatmap(int id)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                APIBeatmap? beatmap = db.Beatmaps.FirstOrDefault(b => b.Id == id);
                return beatmap;
            }
        }

        /// <summary>
        /// Insert beatmap data into the DB
        /// </summary>
        /// <param name="beatmap">Populated APIBeatmap object</param>
        public void InsertBeatmap(APIBeatmap beatmap)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Beatmaps.Add(beatmap);
                db.SaveChanges();
            }
        }
    }
}
