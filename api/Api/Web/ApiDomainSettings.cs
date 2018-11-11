using System;
using Api.Controllers;

namespace Api.Web
{
    /// <summary>
    ///     Setting for the api authorization header
    ///     TODO: this needs to be injected from the request context
    /// </summary>
    /// <example>
    ///     <code>
    ///     "Api":{
    ///          "Domain": "https://api.example.com/"
    ///  </code>
    /// </example>
    public class ApiDomainSettings
    {
        public const string SectionName = "Api";

        /// <summary>
        ///     This is hard coded to <see cref="AuthenticateController.UserAuthenticator" />
        /// </summary>
        private readonly string path = "authenticate/auth0";

        public string Domain { get; set; }

        public string AuthorizationUri()
        {
            return new Uri(new Uri(Domain), path).AbsoluteUri;
        }
    }
}