using Microsoft.Extensions.Configuration;

namespace Api.Web
{
    public static class ConfigurationExtensions
    {
        public static ApiClientSettings GetApiClientSettings(this IConfiguration configuration)
        {
            var settings = new ApiClientSettings();
            configuration.GetSection(ApiClientSettings.SectionName).Bind(settings);
            return settings;
        }
    }
}