using Newtonsoft.Json;

namespace osuscorefetcher.ConfigHandler
{
    internal class ConfigIO
    {
        private static string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appconfig.json");
        public static Config GetConfig()
        {
            Config configFromFile = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return configFromFile;
        }
        public static Config SetConfig(string jsonData) {
            File.WriteAllText(ConfigPath, jsonData);
            return GetConfig();
        }
    }
}
