using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EdlinSoftware.Safe.Services
{
    internal class Configuration
    {
        public string? LastOpenedStorage {  get; set; }
    }

    internal interface IConfigurationService
    {
        Configuration GetConfiguration();

        void SaveConfiguration(Configuration configuration);
    }

    internal sealed class ConfigurationService : IConfigurationService
    {
        public Configuration GetConfiguration()
        {
            var settingsJson = File.ReadAllText("settings.json");

            return JsonSerializer.Deserialize<Configuration>(settingsJson);
        }

        public void SaveConfiguration(Configuration configuration)
        {
            var settingsJson = JsonSerializer.Serialize(configuration);

            File.WriteAllText("settings.json", settingsJson);
        }
    }
}
