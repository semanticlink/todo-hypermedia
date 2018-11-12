using Microsoft.AspNetCore.Mvc;

namespace Api.UriFactory
{
    public static class AuthenticateUriFactory
    {
        /// <summary>
        ///     The route name for the authenticate singleton resource
        /// </summary>
        public const string DefaultRoute = "Authenticate";

        /// <summary>
        ///     The route name for an authenticator resource (eg auth0)
        /// </summary>
        public const string AuthenticatorRouteName = "Authenticator";

        /// <summary>
        ///     The route name for user's specific authenticator singleton resource
        /// </summary>
        public const string UserAuthenticatorRouteName = "UserAuthenticator";

        /// <summary>
        ///     The url for a singleton resource, containing link relations to the authenticator type
        /// </summary>
        public static string MakeAuthenticatorUri(this IUrlHelper url)
        {
            return url.Link(DefaultRoute, new { });
        }

        /// <summary>
        ///     The url for the singleton resource of the authenticator configuration details 
        /// </summary>
        /// <remarks>
        ///    This is an url to get at all the configuration for the client to work an authenticator and is
        ///     exposed publicly
        /// </remarks>
        /// <see cref="Auth0Configuration"/>
        public static string MakeAuthenticatorUri(this string authenticator, IUrlHelper url)
        {
            return url.Link(AuthenticatorRouteName, new {authenticator = authenticator});
        }

        /// <summary>
        ///     The url for a virtual singleton resource for the list off the 'authenticator' rel for the user with title
        /// </summary>
        /// <see cref="Auth0Configuration"/>
        /// <see cref="MakeAuthenticatorUri"/>
        public static string MakeUserAuthenticator(this string authenticatorId, string authenticator, IUrlHelper url)
        {
            return url.Link(UserAuthenticatorRouteName,
                new {authenticatorId = authenticatorId, authenticator = authenticator});
        }
    }
}