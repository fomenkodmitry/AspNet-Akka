using System.IO;
using Akka.Configuration;

namespace Api
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Config Load() => LoadConfig("akka.conf");

        private static Config LoadConfig(string configFile)
        {
            if (File.Exists(configFile))
            {
                string config = File.ReadAllText(configFile);
                return ConfigurationFactory.ParseString(config);
            }


            return Config.Empty;
        }
    }
}