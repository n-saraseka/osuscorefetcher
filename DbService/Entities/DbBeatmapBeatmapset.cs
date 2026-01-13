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
        private DbInstance Instance;
        public DbBeatmapBeatmapset(DbInstance instance) { this.Instance = instance; }

        /// <summary>
        /// Get Beatmapset from the DB
        /// </summary>
        /// <param name="id">Beatmapset ID</param>
        /// <returns>Populated Beatmapset object (or null in case it doesn't exist in the DB)</returns>
        public Beatmapset? GetBeatmapset(int id)
        {
            Beatmapset? beatmapset = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM beatmapset WHERE id=@bid";
                NpgsqlParameter idParam = new NpgsqlParameter("@bid", id);
                command.Parameters.Add(idParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    beatmapset = new Beatmapset();
                    beatmapset.Id = reader.GetInt32(0);
                    beatmapset.Artist = reader.GetString(1);
                    beatmapset.Title = reader.GetString(2);
                    beatmapset.PreviewUrl = reader.GetString(3);
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
            return beatmapset;
        }

        /// <summary>
        /// Insert beatmapset into the DB
        /// </summary>
        /// <param name="beatmapset">Populated Beatmapset object</param>
        /// <returns>Number of inserted rows</returns>
        public int InsertBeatmapset(Beatmapset beatmapset)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "INSERT INTO beatmapset(id, artist, title, preview_url) VALUES (@bid, @at, @t, @pu)";
                NpgsqlParameter idParam = new NpgsqlParameter("@bid", beatmapset.Id);
                NpgsqlParameter artistParam = new NpgsqlParameter("@at", beatmapset.Artist);
                NpgsqlParameter titleParam = new NpgsqlParameter("@t", beatmapset.Title);
                NpgsqlParameter previewUrlParam = new NpgsqlParameter("@pu", beatmapset.PreviewUrl);
                command.Parameters.Add(idParam);
                command.Parameters.Add(artistParam);
                command.Parameters.Add(titleParam);
                command.Parameters.Add(previewUrlParam);
                insertedRows = command.ExecuteNonQuery();

                if (insertedRows > 0) Console.WriteLine($"Inserted beatmapset {beatmapset.Id} into the DB");
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
        /// Get a Beatmap from the DB
        /// </summary>
        /// <param name="id">Beatmap ID</param>
        /// <returns>Populated Beatmap object (or null in case it doesn't exist in the DB)</returns>
        public APIBeatmap? GetBeatmap(int id)
        {
            APIBeatmap? beatmap = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM beatmap WHERE id=@bid";
                NpgsqlParameter idParam = new NpgsqlParameter("@bid", id);
                command.Parameters.Add(idParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    beatmap = new APIBeatmap();
                    beatmap.Id = id;
                    beatmap.BeatmapsetId = reader.GetInt32(0);
                    beatmap.Mode = (Mode)reader.GetInt32(1);
                    beatmap.Difficulty = reader.GetFloat(2);
                    beatmap.BPM = reader.GetFloat(3);
                    beatmap.ApproachRate = reader.GetFloat(4);
                    beatmap.CircleSize = reader.GetFloat(5);
                    beatmap.OverallDifficulty = reader.GetFloat(6);
                    beatmap.DrainLength = reader.GetFloat(7);
                    beatmap.Status = (BeatmapStatus)reader.GetInt32(8);
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
            return beatmap;
        }

        /// <summary>
        /// Insert beatmap data into the DB
        /// </summary>
        /// <param name="beatmap">Populated APIBeatmap object</param>
        /// <returns>Number of inserted rows</returns>
        public int InsertBeatmap(APIBeatmap beatmap)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "INSERT INTO beatmap(id, beatmapset_id, gameplay_mode, difficulty_rating, bpm, ar, cs, accuracy, drain, status) VALUES (";

                // because there are too many parameters...
                List<NpgsqlParameter> parameters = new();
                foreach (PropertyInfo property in beatmap.GetType().GetProperties())
                {
                    command.CommandText += $"@{property.Name}, ";
                    parameters.Add(new NpgsqlParameter($"@{property.Name}", property.GetValue(beatmap)));
                }
                command.CommandText = command.CommandText.Substring(command.CommandText.Length - 2);
                command.CommandText += ")";

                for (int i = 0; i < parameters.Count; i++) command.Parameters.Add(parameters[i]);

                insertedRows = command.ExecuteNonQuery();
                if (insertedRows > 0) Console.WriteLine($"Inserted beatmap {beatmap.Id} into the DB");
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
