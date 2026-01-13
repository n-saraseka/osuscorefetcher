using Npgsql;
using osu.Game.Rulesets;
using osuscorefetcher.ApiClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService.Entities
{
    internal class DbUser
    {
        /// <summary>
        /// Get relevant User object from the DB by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Populated User object (or null in case such user doesn't exist in the DB)</returns>
        public User? GetUserData(int id)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                User? user = db.Users.FirstOrDefault(u => u.Id == id);
                return user;
            }
        }
        /// <summary>
        /// Insert user data into the DB
        /// </summary>
        /// <param name="user">Populated User object</param>
        public void InsertUserData(User user)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Update user data into the DB
        /// </summary>
        /// <param name="user">Populated User object</param>
        public void UpdateUserData(User user)
        {
            using (ScoreFetcherContext db = new ScoreFetcherContext())
            {
                db.Users.Update(user);
                db.SaveChanges();
            }
        }
    }
}
