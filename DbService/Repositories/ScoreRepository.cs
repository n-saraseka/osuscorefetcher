using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.DbService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Entities
{
    public class ScoreRepository : IRepository<Score>
    {
        private ScoreFetcherContext db;

        public ScoreRepository()
        {
            this.db = new ScoreFetcherContext();
        }

        public IEnumerable<Score> GetAll()
        {
            return db.Scores;
        }

        public Score? Get(int id)
        {
            return db.Scores.Find(id);
        }

        public void Create(Score score)
        {
            db.Scores.Add(score);
        }

        public void CreateBulk(IEnumerable<Score> scores)
        {
            db.Scores.AddRange(scores);
        }

        public void Update(Score score)
        {
            db.Entry(score).State = EntityState.Modified;
        }

        public void UpdateBulk(IEnumerable<Score> scores)
        {
            foreach (Score score in scores)
                Update(score);
        }

        public void Delete(int id)
        {
            Score? score = Get(id);
            if (score != null)
                db.Scores.Remove(score);
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
