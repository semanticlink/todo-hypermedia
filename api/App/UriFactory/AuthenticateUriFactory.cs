using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class AuthenticateUriFactory
    {
        public const string DefaultRoute = "AuthenticateRouteName";
        public const string Auth0RouteName = "AuthenticateAuth0RouteName";

        public static string MakeAuthenicateUri(this IUrlHelper url)
        {
            return url.Link(DefaultRoute, new { });
        }

        public static string MakeAuthenicateAuth0Uri(this IUrlHelper url)
        {
            return url.Link(Auth0RouteName, new { });
        }
    }
}