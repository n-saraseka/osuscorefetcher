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
        private DbInstance Instance;

        public DbUser(DbInstance instance) { this.Instance = instance; }

        /// <summary>
        /// Get relevant User object from the DB by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Populated User object (or null in case such user doesn't exist in the DB)</returns>
        public User? GetUserData(int id)
        {
            User? userData = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM osu_user";
                NpgsqlParameter idParam = new NpgsqlParameter("@user_id", id);
                command.Parameters.Add(idParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int userId = reader.GetInt32(0);
                    string username = reader.GetString(1);
                    string countryCode = reader.GetString(2);

                    userData = new User();
                    userData.Id = userId;
                    userData.Username = username;

                    DbCountry countryData = new(new DbInstance());
                    Country? userCountry = countryData.GetCountry(countryCode);
                    userData.Country = userCountry;

                    DbUserRulesetStatistics statisticsData = new(new DbInstance());
                    userData.RulesetStatistics = statisticsData.GetAllUserStatistics(userId);
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database exception: {ex.Message}");
            }
            finally
            {
                Instance.Connection.Close();
            }
            return userData;
        }
        /// <summary>
        /// Insert user data into the DB
        /// </summary>
        /// <param name="user">Populated User object</param>
        /// <returns>Number of inserted rows</returns>
        public int InsertUserData(User user)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();

                // check if db has country data, insert otherwise
                DbCountry countryData = new(new DbInstance());
                Country? userCountry = countryData.GetCountry(user.Country.Code);
                if (userCountry == null)
                    insertedRows += countryData.InsertCountry(user.Country);

                // insert user data
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "INSERT INTO osu_user(id, username, country_code) VALUES (@user_id, @user_username, @country_code)";
                NpgsqlParameter idParam = new NpgsqlParameter("@user_id", user.Id);
                NpgsqlParameter usernameParam = new NpgsqlParameter("@user_username", user.Username);
                NpgsqlParameter countryCodeParam = new NpgsqlParameter("@country_code", user.Country.Code);
                command.Parameters.Add(idParam);
                command.Parameters.Add(usernameParam);
                command.Parameters.Add(countryCodeParam);
                insertedRows += command.ExecuteNonQuery();

                if (insertedRows > 0) Console.WriteLine($"Inserted user data for user {user.Id} into the DB");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database exception: {ex.Message}");
            }
            finally
            {
                Instance.Connection.Close();
            }
            return insertedRows;
        }
        /// <summary>
        /// Update user data into the DB
        /// </summary>
        /// <param name="user">Populated User object</param>
        /// <returns>Number of updated rows</returns>
        public int UpdateUserData(User user)
        {
            int updatedRows = 0;
            try
            {
                Instance.Connection.Open();
                // check if db has country data, insert otherwise
                // its still relevant cause flag changes duh
                DbCountry countryData = new(Instance);
                Country? userCountry = countryData.GetCountry(user.Country.Code);
                if (userCountry == null)
                    updatedRows += countryData.InsertCountry(user.Country);

                // update user data
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "UPDATE osu_user SET username=@user_username, country_code=@country_code WHERE id=@user_id)";
                NpgsqlParameter idParam = new NpgsqlParameter("@user_id", user.Id);
                NpgsqlParameter usernameParam = new NpgsqlParameter("@user_username", user.Username);
                NpgsqlParameter countryCodeParam = new NpgsqlParameter("@country_code", user.Country.Code);
                command.Parameters.Add(idParam);
                command.Parameters.Add(usernameParam);
                command.Parameters.Add(countryCodeParam);
                updatedRows += command.ExecuteNonQuery();

                if (updatedRows > 0) Console.WriteLine($"Updated user data for user {user.Id} into the DB");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database exception: {ex.Message}");
            }
            finally
            {
                Instance.Connection.Close();
            }
            return updatedRows;
        }
    }
}
