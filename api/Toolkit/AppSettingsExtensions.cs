using System;
using JetBrains.Annotations;
using NLog;
using System.Configuration;
using System.Linq;

namespace Toolkit
{
    /// <summary>
    ///     Support for getting application settings from the application settings file
    ///     as well as supporting environment variable overrides.
    /// </summary>
    public static class AppSettingsExtensions
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string AppSetting([NotNull] this string key, string defaultValue)
        {
            return AppSetting(key, () => defaultValue);
        }

        /// <summary>
        ///     Get an application setting, or a 'null' value if it isn't present.
        /// </summary>
        public static string AppSettingOrNull([NotNull] this string key)
        {
            return AppSetting(key, () => null);
        }

        /// <summary>
        ///     Get an application setting. This will throw an error, rather than returning no setting.
        /// </summary>
        [NotNull]
        public static string AppSetting([NotNull] this string key)
        {
            return AppSetting(key,
                () =>
                {
                    throw new ConfigurationErrorsException(
                        $"The application setting [{key}] is not provided or is empty.");
                });
        }

        public static string AppSetting([NotNull] this string key, Func<string> defaultValue)
        {
            var env = Environment.GetEnvironmentVariable(key);
            if (env != null)
            {
                return env;
            }

            var app = ConfigurationManager.AppSettings.GetValues(key);
            if (app != null && app.Any())
            {
                return app.First();
            }

            return defaultValue?.Invoke();
        }

        public static int AppSetting([NotNull] this string key, int defaultValue)
        {
            int theValue;
            var env = Environment.GetEnvironmentVariable(key);
            if (env != null)
            {
                if (int.TryParse(env, out theValue))
                {
                    return theValue;
                }
            }

            var app = ConfigurationManager.AppSettings.GetValues(key);
            if (app != null && app.Any())
            {
                if (int.TryParse(app.First(), out theValue))
                {
                    return theValue;
                }
            }

            return defaultValue;
        }

        public static bool BoolAppSetting([NotNull] this string key)
        {
            return key.BoolAppSetting(false);
        }

        public static bool BoolAppSetting([NotNull] this string key, bool defaultValue)
        {
            bool theValue;
            var env = Environment.GetEnvironmentVariable(key);
            if (env != null)
            {
                if (bool.TryParse(env, out theValue))
                {
                    return theValue;
                }
            }

            var app = ConfigurationManager.AppSettings.GetValues(key);
            if (app != null && app.Any())
            {
                if (bool.TryParse(app.First(), out theValue))
                {
                    return theValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        ///     This is the base connection string fetcher used by all applicationss.
        /// </summary>
        /// <param name = "name">
        ///     The name of the connection string. The named connection must be present
        ///     in the 'ConnectionString' section of the application configuration file.
        /// </param>
        /// <returns>The raw connection string without replacement parameters being rewritten.</returns>
        [NotNull]
        public static string GetConnectionStringByName([NotNull] this string name)
        {
            // Look for the name in the connectionStrings section.
            return ConfigurationManager
                .ConnectionStrings[name]
                .ThrowConfigurationErrorsExceptionIfNull(() =>
                    $"The connection string '{name}' is not available. The following {ConfigurationManager.ConnectionStrings.Count} are available: {ConfigurationManager.ConnectionStrings.ToCsvString<ConnectionStringSettings>(s => s.Name)}")
                .ConnectionString
                .ThrowConfigurationErrorsExceptionIfNullOrWhiteSpace(
                    $"The connection string for connection '{name}' is empty");
        }

        /// <summary>
        ///     Get a database connection string from the 'ConnectionStrings.config' and perform
        ///     parameter replacement (from the app settings and the environment).
        /// </summary>
        /// <seealso cref = "GetConnectionStringByName" />
        /// <seealso cref = "ReplaceParameters" />
        [NotNull]
        public static string GetConnectionStringByNameWithReplacementparameters([NotNull] this string name)
        {
            return name
                .GetConnectionStringByName()
                .ReplaceParameters()
                .ThrowConfigurationErrorsExceptionIfNullOrWhiteSpace(
                    $"The connection string for connection '{name}' is empty");
        }
    }
}