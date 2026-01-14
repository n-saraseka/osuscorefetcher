using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using osuscorefetcher.ApiClasses;

namespace osuscorefetcher.DbService.Entities
{
    internal class DbCountry
    {
        /// <summary>
        /// Get Country from the DB
        /// </summary>
        /// <param name="code">Country code</param>
        /// <returns>Populated Country object (or null in case such country doesn't exist in the DB)</returns>
        public Country? GetCountry(string code)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                Country? country = db.Countries.FirstOrDefault(c => c.Code == code);
                return country;
            }
        }
        /// <summary>
        /// Insert country data into the DB
        /// </summary>
        /// <param name="country">Populated Country object</param>
        public void InsertCountry(Country country)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Countries.Add(country);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Insert data for multiple countries into the DB
        /// </summary>
        /// <param name="countries">IEnumerable containing populated Country objects</param>
        public void InsertCountries(IEnumerable<Country> countries)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Countries.AddRange(countries);
                db.SaveChanges();
            }
        }
    }
}
