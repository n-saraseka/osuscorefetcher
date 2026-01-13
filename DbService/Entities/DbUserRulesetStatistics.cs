using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using osuscorefetcher.ApiClasses;

namespace osuscorefetcher.DbService.Entities
{
    internal class DbUserRulesetStatistics
    {
        private DbInstance Instance;
        public DbUserRulesetStatistics(DbInstance instance) { this.Instance = instance; }

        public Dictionary<string, UserRulesetStatistics>? GetAllUserStatistics(int user_id)
        {
            Dictionary<string, UserRulesetStatistics>? userStatisticsData = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM user_ruleset_statistics WHERE user_id=@uid";
                NpgsqlParameter userIdParam = new NpgsqlParameter("@uid", user_id);
                command.Parameters.Add(userIdParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    userStatisticsData = new();
                    while (reader.Read())
                    {
                        int mode = reader.GetInt32(1);
                        int globalRank = reader.GetInt32(2);
                        int PP = reader.GetInt32(3);
                        string modeString = Enum.GetName(typeof(Mode), mode).ToLower();

                        UserRulesetStatistics rulesetStatistics = new();
                        rulesetStatistics.GlobalRank = globalRank;
                        rulesetStatistics.PP = PP;

                        userStatisticsData.Add(modeString, rulesetStatistics);
                    }
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
            return userStatisticsData;
        }

        /// <summary>
        /// Get user's global and country rank from the DB by their ID
        /// </summary>
        /// <param name="user_id">User ID</param>
        /// <param name="ruleset">Ruleset</param>
        /// <returns>Populated UserRulesetStatistics object (or null in case user statistics data doesn't exist in the DB)</returns>
        public UserRulesetStatistics? GetUserStatisticsPerRuleset(int user_id, Mode ruleset)
        {
            UserRulesetStatistics? userStatisticsData = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM user_rank WHERE user_id=@uid AND gameplay_mode=@rid";
                NpgsqlParameter userIdParam = new NpgsqlParameter("@uid", user_id);
                NpgsqlParameter rulesetIdParam = new NpgsqlParameter("@rid", (int)ruleset);
                command.Parameters.Add(userIdParam);
                command.Parameters.Add(rulesetIdParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int globalRank = reader.GetInt32(2);
                    int pp = reader.GetInt32(3);
                    userStatisticsData = new UserRulesetStatistics();
                    userStatisticsData.GlobalRank = globalRank;
                    userStatisticsData.PP = pp;
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
            return userStatisticsData;
        }
        /// <summary>
        /// Insert user's global and country rank data to the DB
        /// </summary>
        /// <param name="user_id">User ID</param>
        /// <param name="ruleset">Ruleset</param>
        /// <param name="statistics">User's global rank and PP data</param>
        /// <returns>Number of inserted rows</returns>
        public int InsertUserRulesetStatistics(int user_id, Mode ruleset, UserRulesetStatistics statistics)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "INSERT INTO user_rank(user_id, gameplay_mode, global_rank, pp) VALUES (@uid, @rid, @global, @pps)";
                NpgsqlParameter userIdParam = new NpgsqlParameter("@uid", user_id);
                NpgsqlParameter rulesetIdParam = new NpgsqlParameter("@rid", (int)ruleset);
                NpgsqlParameter globalRankParam = new NpgsqlParameter("@global", statistics.GlobalRank);
                NpgsqlParameter ppParam = new NpgsqlParameter("@pps", statistics.PP);
                command.Parameters.Add(userIdParam);
                command.Parameters.Add(rulesetIdParam);
                command.Parameters.Add(globalRankParam);
                command.Parameters.Add(ppParam);
                insertedRows = command.ExecuteNonQuery();
                if (insertedRows > 0) Console.WriteLine($"Inserted {ruleset} rank and PP data for user {user_id} into the DB");
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
        /// Update user's global and country rank data to the DB
        /// </summary>
        /// <param name="user_id">User ID</param>
        /// <param name="ruleset">User ID</param>
        /// <param name="statistics">User's global rank and PP data</param>
        /// <returns>Number of updated rows</returns>
        public int UpdateUserRulesetStatistics(int user_id, Mode ruleset, UserRulesetStatistics statistics)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "Update user_rank SET global_rank=@global, pp=@pps WHERE user_id=@uid AND gameplay_mode=@rid";
                NpgsqlParameter userIdParam = new NpgsqlParameter("@uid", user_id);
                NpgsqlParameter rulesetIdParam = new NpgsqlParameter("@rid", (int)ruleset);
                NpgsqlParameter globalRankParam = new NpgsqlParameter("@global", statistics.GlobalRank);
                NpgsqlParameter ppParam = new NpgsqlParameter("@country", statistics.PP);
                command.Parameters.Add(userIdParam);
                command.Parameters.Add(rulesetIdParam);
                command.Parameters.Add(globalRankParam);
                command.Parameters.Add(ppParam);
                insertedRows = command.ExecuteNonQuery();
                if (insertedRows > 0) Console.WriteLine($"Updated {ruleset} rank and PP data for user {user_id} into the DB");
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
    }
}
