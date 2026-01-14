using Microsoft.EntityFrameworkCore;
using osuscorefetcher.ApiClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Repositories
{
    public class BeatmapsetRepository : IRepository<Beatmapset>
    {
        private ScoreFetcherContext db;

        public BeatmapsetRepository()
        {
            this.db = new ScoreFetcherContext();
        }

        public BeatmapsetRepository(ScoreFetcherContext db)
        {
            this.db = db;
        }

        public IEnumerable<Beatmapset> GetAll()
        {
            return db.Beatmapsets;
        }

        public Beatmapset? Get(int id)
        {
            return db.Beatmapsets.Find(id);
        }

        public void Create(Beatmapset beatmapset)
        {
            db.Beatmapsets.Add(beatmapset);
        }

        public void CreateBulk(IEnumerable<Beatmapset> beatmapsets)
        {
            db.Beatmapsets.AddRange(beatmapsets);
        }

        public void Update(Beatmapset beatmapset)
        {
            db.Entry(beatmapset).State = EntityState.Modified;
        }

        public void UpdateBulk(IEnumerable<Beatmapset> beatmapsets)
        {
            foreach (Beatmapset beatmapset in beatmapsets)
                Update(beatmapset);
        }

        public void Delete(int id)
        {
            Beatmapset? beatmapset = Get(id);
            if (beatmapset != null)
                db.Beatmapsets.Remove(beatmapset);
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
