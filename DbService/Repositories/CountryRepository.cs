using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    public class CountryRepository : IDisposable
    {
        private ScoreFetcherContext db;

        public CountryRepository()
        {
            this.db = new ScoreFetcherContext();
        }

        public CountryRepository(ScoreFetcherContext db)
        {
            this.db = db;
        }

        public IEnumerable<Country> GetAll()
        {
            return db.Countries;
        }

        public Country? Get(string code)
        {
            return db.Countries.Find(code);
        }

        public void Create(Country country)
        {
            db.Countries.Add(country);
        }

        public void CreateBulk(IEnumerable<Country> countries)
        {
            db.Countries.AddRange(countries);
        }

        public void Update(Country country)
        {
            db.Entry(country).State = EntityState.Modified;
        }

        public void UpdateBulk(IEnumerable<Country> countries)
        {
            foreach (Country country in countries)
                Update(country);
        }

        public void Delete(string code)
        {
            Country? country = Get(code);
            if (country != null)
                db.Countries.Remove(country);
        }

        public void DeleteBulk(IEnumerable<string> codes)
        {
            foreach (string code in codes)
                Delete(code);
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
