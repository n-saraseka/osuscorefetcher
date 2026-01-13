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
        private DbInstance Instance;
        public DbCountry(DbInstance instance) { this.Instance = instance; }
        /// <summary>
        /// Get Country from the DB
        /// </summary>
        /// <param name="code">Country code</param>
        /// <returns>Populated Country object (or null in case such country doesn't exist in the DB)</returns>
        public Country? GetCountry(string code)
        {
            Country? country = null;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "SELECT * FROM country WHERE code=@country_code";
                NpgsqlParameter countryCodeParam = new NpgsqlParameter("@country_code", code);
                command.Parameters.Add(countryCodeParam);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    country = new Country();
                    country.Code = reader.GetString(0);
                    country.Name = reader.GetString(1);
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
            return country;
        }
        /// <summary>
        /// Insert country data into the DB
        /// </summary>
        /// <param name="country">Populated Country object</param>
        /// <returns>Number of inserted rows</returns>
        public int InsertCountry(Country country)
        {
            int insertedRows = 0;
            try
            {
                Instance.Connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.CommandText = "INSERT INTO country(code, name) VALUES (@country_code, @country_name)";
                NpgsqlParameter countryCodeParam = new NpgsqlParameter("@country_code", country.Code);
                NpgsqlParameter countryNameParam = new NpgsqlParameter("@country_name", country.Name);
                command.Parameters.Add(countryCodeParam);
                command.Parameters.Add(countryNameParam);
                insertedRows = command.ExecuteNonQuery();
                if (insertedRows > 0) Console.WriteLine($"Inserted country {country.Code} into the DB");
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
