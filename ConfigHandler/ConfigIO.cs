using Newtonsoft.Json;

namespace osuscorefetcher.ConfigHandler
{
    internal class ConfigIO
    {
        private static string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appconfig.json");

        /// <summary>
        /// Get latest Config object from ConfigPath
        /// </summary>
        /// <returns>Populated Config object</returns>
        public static Config GetConfig()
        {
            Config configFromFile = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return configFromFile;
        }
        
        /// <summary>
        /// Save new config data from JSON string
        /// </summary>
        /// <param name="jsonData">JSON config data</param>
        /// <returns>Updated Config object</returns>
        public static Config SetConfig(string jsonData) {
            File.WriteAllText(ConfigPath, jsonData);
            return GetConfig();
        }
    }
}
