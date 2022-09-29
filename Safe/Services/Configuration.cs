using System.IO;
using System.Text.Json;

namespace EdlinSoftware.Safe.Services
{
    internal class Configuration
    {
        public string? LastOpenedStorage {  get; set; }

        public string Language { get; set; } = "en-US";
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
