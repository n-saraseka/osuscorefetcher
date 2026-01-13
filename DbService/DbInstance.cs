using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using osuscorefetcher.ConfigHandler;
using Npgsql;

namespace osuscorefetcher.DbService
{
    public class DbInstance
    {
        private static readonly Config Config = ConfigIO.GetConfig();
        private static readonly string ConnectionString = $"Host={Config.DbHost};Username={Config.DbUsername};Password={Config.DbPassword};Database={Config.DbName}";
        public NpgsqlConnection Connection { get; private set; }
        public DbInstance()
        {
            Connection = new NpgsqlConnection(ConnectionString);
        }

    }
}
