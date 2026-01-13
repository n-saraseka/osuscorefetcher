using Npgsql;
using osu.Game.Beatmaps;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService
{
    internal class Utils
    {
        /// <summary>
        /// Generate a non-query (INSERT/UPDATE) command.
        /// </summary>
        /// <param name="isInsert">Whether the command is INSERT or not</param>
        /// <param name="tableName">Name of the table that's being inserted into or updated</param>
        /// <param name="parameterNames">A string array containing all the column names</param>
        /// <param name="objectWithProperties">An object with properties. Those properties are used to create command parameters with respective values</param>
        /// <returns>NpgsqlCommand object with filled out parameters. VERY IMPORTANT: WHERE statements with their parameters have to be added to the resulting command manually.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static NpgsqlCommand GenNonQueryCommand(
            bool isInsert, string tableName, string[] columnNames, object objectWithProperties)
        {
            PropertyInfo[] properties = objectWithProperties.GetType().GetProperties();
            if (columnNames.Length != properties.Length)
                throw new ArgumentException("There should be as many columns as there are properties");

            string sql;
            List<NpgsqlParameter> parameters = new();

            if (isInsert)
            {
                sql = $"INSERT INTO {tableName}(";
                for (int i = 0; i < columnNames.Length; i++) sql += $"{columnNames[i]}, ";
                sql = sql.Substring(sql.Length - 2) + ") VALUES (";
                
                for (int i = 0; i < columnNames.Length; i++) 
                {
                    sql += $"@{properties[i].Name}, ";
                    parameters.Add(new NpgsqlParameter($"@{properties[i].Name}", properties[i].GetValue(objectWithProperties)));
                }

                sql = sql.Substring(sql.Length - 2) + ")";
            }
            else {
                sql = $"UPDATE {tableName} SET ";
                for (int i = 0; i < columnNames.Length; i++)
                {
                    sql += $"{columnNames[i]}=@{properties[i].Name}, ";
                    parameters.Add(new NpgsqlParameter($"@{properties[i].Name}", properties[i].GetValue(objectWithProperties)));
                }
                sql = sql.Substring(sql.Length - 2);
            }
            NpgsqlCommand command = new();
            command.CommandText = sql;
            for (int i = 0; i < parameters.Count; i++) command.Parameters.Add(parameters[i]);
            return command;
        }
    }
}
