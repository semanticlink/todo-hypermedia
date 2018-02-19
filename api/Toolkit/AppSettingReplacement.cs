namespace Toolkit
{
    using System;
    using System.Configuration;
    using System.Text.RegularExpressions;
    using NLog;


    public static class AppSettingReplacement
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Match parameters of the form:
        ///     '${' name [ ':' default ] '}'
        ///     where name is a replacement name, and default is a default value if
        ///     name can't be found as a replacement variable.
        /// </summary>
        private const string ParameterRegularExpression =
            @"
                \$ # Must start with a dollar literal
                \{ # opening bracket literal

                    # name of app setting or the environment variable
                    (?<name> [\w-._]+ )

                    # default value
                    ( \: (?<default> [^:}]* ) )?

                \} # closing bracket literal
                ";

        /// <summary>
        ///     Replace parameters in <paramref name = "appSettingReplacementString" /> with values from
        ///     the environment or app settings.
        /// </summary>
        /// <remarks>
        ///     Replace ment parameters are of the form '${name}'.
        /// </remarks>
        public static string ReplaceParameters(this string appSettingReplacementString)
        {
            return !string.IsNullOrEmpty(appSettingReplacementString)
                ? Regex.Replace(
                    appSettingReplacementString,
                    ParameterRegularExpression,
                    OnMatch,
                    RegexOptions.CultureInvariant |
                    RegexOptions.Compiled |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.Multiline)
                : appSettingReplacementString;
        }

        private static string OnMatch(Match m)
        {
            if (m.Groups["name"].Success)
            {
                var name = m.Groups["name"].Value;
                var env = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrEmpty(env))
                {
                    return env;
                }

                var app = ConfigurationManager.AppSettings[name];
                if (!string.IsNullOrEmpty(app))
                {
                    return app;
                }

                var defaultGroup = m.Groups["default"];
                var defaultValue = defaultGroup.Success ? defaultGroup.Value : name;
                Log.Trace(
                    "Application setting '{0}' has no value, using the default value '{1}'",
                    name,
                    defaultValue);
                return defaultValue;
            }

            Log.Trace("A match was found with no 'name' match");
            return string.Empty;
        }
    }
}