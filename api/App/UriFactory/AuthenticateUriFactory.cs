using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class AuthenticateUriFactory
    {
        public const string DefaultRoute = "AuthenticateRouteName";
        public const string AuthenticatorRouteName = "AuthenticatorRouteName";
        public const string UserAuthenticatorRouteName = "UserAuthenticatorRouteName";

        public static string MakeAuthenicateUri(this IUrlHelper url)
        {
            return url.Link(DefaultRoute, new { });
        }

        public static string MakeAuthenicatorUri(this string authenticator, IUrlHelper url)
        {
            return url.Link(AuthenticatorRouteName, new { authenticator = authenticator });
        }
        
        public static string MakeUserAuthenticator(this string authenticatorId, string authenticator, IUrlHelper url)
        {
            return url.Link(UserAuthenticatorRouteName, new {authenticatorId = authenticatorId, authenticator = authenticator});
        }


    }
}