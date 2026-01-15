using Microsoft.EntityFrameworkCore;
using Npgsql;
using osu.Game.Rulesets;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.DbService.Repositories;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Entities
{
    public class UserRepository : IRepository<User>
    {
        private ScoreFetcherContext db;

        public UserRepository()
        {
            this.db = new ScoreFetcherContext();
        }

        public IEnumerable<User> GetAll()
        {
            return db.Users;
        }

        public User? Get(int id)
        {
            return db.Users.Find(id);
        }

        public void Create(User user)
        {
            using CountryRepository countryRepository = new CountryRepository(db);
            Country? userCountry = countryRepository.Get(user.CountryCode);
            if (userCountry == null)
            {
                countryRepository.Create(user.Country);
                countryRepository.Save();
            }
            countryRepository.Dispose();

            db.Users.Add(user);
        }

        public void CreateBulk(IEnumerable<User> users)
        {
            CountryRepository countryRepository = new CountryRepository(db);
            List<Country> userCountries = users
                .GroupBy(u => u.CountryCode)
                .Select(g => g.First().Country)
                .ToList();
            List<string> existingCountryCodes = countryRepository.GetAll().Select(c => c.Code).ToList();
            List<Country> newCountries = userCountries.Where(c => !existingCountryCodes.Contains(c.Code)).ToList();
            if (newCountries.Count > 0)
            {
                countryRepository.CreateBulk(newCountries);
                countryRepository.Save();
            }

            List<Country> existingCountries = countryRepository.GetAll().ToList();
            foreach (User user in users)
            {
                user.Country = existingCountries.FirstOrDefault(c => c.Code == user.Country.Code);
            }

            db.Users.AddRange(users);
        }

        public void Update(User user)
        {
            db.Entry(user).State = EntityState.Modified;
        }

        public void UpdateBulk(IEnumerable<User> users)
        {
            foreach (User user in users)
                Update(user);
        }

        public void Delete(int id)
        {
            User? user = Get(id);
            if (user != null)
                db.Users.Remove(user);
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
