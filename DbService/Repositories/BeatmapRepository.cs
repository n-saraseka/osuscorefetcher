using Microsoft.EntityFrameworkCore;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.DbService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Entities
{
    public class BeatmapRepository : IRepository<APIBeatmap>
    {
        private ScoreFetcherContext db;

        public BeatmapRepository()
        {
            this.db = new ScoreFetcherContext();
        }

        public IEnumerable<APIBeatmap> GetAll()
        {
            return db.Beatmaps;
        }

        public APIBeatmap? Get(int id)
        {
            return db.Beatmaps.Find(id);
        }

        public void Create(APIBeatmap beatmap)
        {
            using BeatmapsetRepository beatmapsetRepository = new BeatmapsetRepository(db);
            Beatmapset? beatmapset = beatmapsetRepository.Get(beatmap.BeatmapsetId);
            if (beatmapset != null)
                beatmap.Beatmapset = beatmapset;

            db.Beatmaps.Add(beatmap);
        }

        public void CreateBulk(IEnumerable<APIBeatmap> beatmaps)
        {
            BeatmapsetRepository beatmapsetRepository = new BeatmapsetRepository(db);
            List<Beatmapset> beatmapsets = beatmaps
                .GroupBy(u => u.BeatmapsetId)
                .Select(g => g.First().Beatmapset)
                .ToList();
            List<int> existingBeatmapsetsIds = beatmapsetRepository.GetAll().Select(bs => bs.Id).ToList();
            List<Beatmapset> newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsetsIds.Contains(bs.Id)).ToList();
            if (newBeatmapsets.Count > 0)
            {
                beatmapsetRepository.CreateBulk(newBeatmapsets);
                beatmapsetRepository.Save();
            }

            List<Beatmapset> existingBeatmapsets = beatmapsetRepository.GetAll().ToList();
            foreach (APIBeatmap beatmap in beatmaps)
            {
                beatmap.Beatmapset = existingBeatmapsets.FirstOrDefault(bs => bs.Id == beatmap.Beatmapset.Id);
            }

            db.Beatmaps.AddRange(beatmaps);
        }

        public void Update(APIBeatmap beatmap)
        {
            db.Entry(beatmap).State = EntityState.Modified;
        }

        public void UpdateBulk(IEnumerable<APIBeatmap> beatmaps)
        {
            foreach (APIBeatmap beatmap in beatmaps)
                Update(beatmap);
        }

        public void Delete(int id) {
            APIBeatmap? beatmap = Get(id);
            if (beatmap != null)
                db.Beatmaps.Remove(beatmap);
        }

        public void DeleteBulk(IEnumerable<int> ids)
        {
            foreach (int id in ids)
                Delete(id);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
