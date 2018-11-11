using System;
using System.Collections.Generic;
using System.Linq;

namespace SemanticLink.AspNetCore
{
    /// <summary>
    ///     Setting for the api client html to pick the external javascript
    /// </summary>
    /// <example>
    ///
    ///  Development is likely to only have one:
    ///
    /// <code>
    ///    "Api.Client":{
    ///         "Scripts": ["dist/api.js"],
    ///         "Domain": "http://localhost:8080/"
    ///     }
    ///  </code>    
    ///  Production may have multiple to allow for Progressive web apps:
    ///
    /// <code>
    ///    "Api.Client":{
    ///         "Scripts": ["api.js", "vendors~api.js", "vendors~api~app.js"],
    ///         "Domain": "https://api.example.com/"
    /// </code>    
    ///</example>
    public class ApiClientSettings
    {
        public const string SectionName = "Api.Client";
        public List<string> Scripts { get; set; }
        public string Domain { get; set; }

        public List<string> ToScriptsHref() => Scripts
            .Select(script => new Uri(new Uri(Domain), script).AbsoluteUri)
            .ToList();
    }
}