using System.ComponentModel.DataAnnotations;

namespace ClientRestServer.Configuration
{
    public sealed class ConfigurationServerOptions
    {
        public const string SECTION_NAME = "configurationServer";

        [Required, Url]
        public string Url { get; init; }

    }
}
