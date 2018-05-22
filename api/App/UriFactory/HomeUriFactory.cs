using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class HomeUriFactory
    {
        public const string SelfRouteName = "Home";
        public const string AuthenticateRouteName = "Authenticate";

        /// <summary>
        ///     A logical resource that is the collection of all tenants
        /// </summary>
        public const string TenantsRouteName = "HomeTenants";

        public const string HomeTenantSearchRouteName = "HomeTenantSearch";
        public const string HomeTenantSearchFormRouteName = "HomeTenantSearchForm";

        public static string MakeHomeUri(this IUrlHelper url)
        {
            return url.Link(SelfRouteName, new { });
        }

         public static string MakeAuthenticateUri(this IUrlHelper url)
        {
            return url.Link(AuthenticateRouteName, new { });
        }

        /// <summary>
        ///     A collection resource for the list of tenants, given a string search criteria
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
        ///     A collection resource for the list of tenants (without a search criteria).
        /// </summary>
        /// <remarks>
        ///     From a disclosure point-of-view this list is not presented via the API to
        ///     non-administrators.
        /// </remarks>
        public static string MakeHomeTenantsUri(this IUrlHelper url)
        {
            return url.Link(TenantsRouteName, new {});
        }

        /// <summary>
        ///     The Url for POSTing a search form
        /// </summary>
        /// <remarks>
        ///     This is <b>not</b> the URL of the collection as we use this for create
        ///     and we can't easily multiplex based on the POST data content
        /// </remarks>
        public static string MakeHomeTenantSearchUri(this IUrlHelper url)
        {
            return url.Link(HomeTenantSearchRouteName, new { });
        }


        public static string MakeHomeTenantsSearchFormUri(this IUrlHelper url)
        {
            return url.Link(HomeTenantSearchFormRouteName, new { });
        }
    }
}