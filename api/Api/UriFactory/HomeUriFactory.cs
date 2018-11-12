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
        ///     The route name for a logical resource that is the collection of all searchable tenants
        /// </summary>
        public const string TenantsRouteName = "HomeTenants";

        /// <summary>
        ///     The route name for a logical resource that is the collection of all users
        /// </summary>
        public const string UsersRouteName = "HomeUsers";

        /// <summary>
        ///     The route name for a logical resource that is a search collection of all searchable tenants
        /// </summary>
        public const string HomeTenantSearchRouteName = "HomeTenantSearch";
        
        /// <summary>
        ///     The route name for a tenant search form resource
        /// </summary>
        public const string HomeTenantSearchFormRouteName = "HomeTenantSearchForm";

        /// <summary>
        ///     The url of the home/root of the API
        /// </summary>
        public static string MakeHomeUri(this IUrlHelper url)
        {
            return url.Link(DefaultRoute, new { });
        }

        /// <summary>
        ///     The url for a collection resource for the list of tenants, given a string search criteria
        /// </summary>
        /// <remarks>
        ///     From a disclosure point-of-view this list is not presented via the API to
        ///     non-administrators.
        /// </remarks>
        public static string MakeHomeTenantsUri(this string searchCriteria, IUrlHelper url)
        {
            return url.Link(TenantsRouteName, new {q = searchCriteria});
        }

        /// <summary>
        ///     The url for a collection resource for the list of tenants (without a search criteria).
        /// </summary>
        /// <remarks>
        ///     From a disclosure point-of-view this list is not presented via the API to
        ///     non-administrators.
        /// </remarks>
        public static string MakeHomeTenantsUri(this IUrlHelper url)
        {
            return url.Link(TenantsRouteName, new { });
        }

        /// <summary>
        ///     The url for POSTing a search form
        /// </summary>
        /// <remarks>
        ///     This is <b>not</b> the URL of the collection as we use this for create
        ///     and we can't easily multiplex based on the POST data content
        /// </remarks>
        public static string MakeHomeTenantSearchUri(this IUrlHelper url)
        {
            return url.Link(HomeTenantSearchRouteName, new { });
        }

        /// <summary>
        ///     The url for the search form for locating a matched tenant for a user
        /// </summary>
        public static string MakeHomeTenantsSearchFormUri(this IUrlHelper url)
        {
            return url.Link(HomeTenantSearchFormRouteName, new { });
        }

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
