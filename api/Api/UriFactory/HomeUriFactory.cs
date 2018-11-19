using Microsoft.AspNetCore.Mvc;

namespace Api.UriFactory
{
    public static class HomeUriFactory
    {
        /// <summary>
        ///     The route name for home/root of the API
        /// </summary>
        public const string DefaultRoute = "Home";


        /// <summary>
        ///     The url of the home/root of the API
        /// </summary>
        public static string MakeHomeUri(this IUrlHelper url)
        {
            return url.Link(DefaultRoute, new { });
        }
        
        /// <summary>
        ///     The route name for a logical resource that is the collection of all users
        /// </summary>
        public const string UsersRouteName = "HomeUsers";

        /// <summary>
        ///     The url for a collection resource for the list of users.
        /// </summary>
        /// <remarks>
        ///     From a disclosure point-of-view this list is not presented via the API to
        ///     non-administrators.
        /// </remarks>
        public static string MakeHomeUsersUri(this IUrlHelper url)
        {
            return url.Link(UsersRouteName, new { });
        }
    }
}
