using Microsoft.AspNetCore.Mvc;

namespace Api.UriFactory
{
    public static class TenantUriFactory
    {
        /// <summary>
        ///     The route name for tenant resource
        /// </summary>
        public const string TenantRouteName = "Tenant";

        /// <summary>
        ///     The route name for an edit form for a tenant
        /// </summary>
        public const string EditFormRouteName = "TenantEditForm";

        /// <summary>
        ///     The route name for a create form for a tenant
        /// </summary>
        public const string CreateFormRouteName = "TenantCreateForm";

        /// <summary>
        ///     The route name for a collection resource of users in the context of a tenant
        /// </summary>
        public const string TenantUsersRouteName = "TenantUsers";

        /// <summary>
        ///     The route name for a logical resource that is the collection of all searchable tenants
        /// </summary>
        public const string TenantsRouteName = "Tenants";

        /// <summary>
        ///     The route name for a logical resource that is a search collection of all searchable tenants
        /// </summary>
        public const string TenantSearchRouteName = "TenantSearch";
        
        /// <summary>
        ///     The route name for a tenant search form resource
        /// </summary>
        public const string TenantSearchFormRouteName = "TenantSearchForm";

        /// <summary>
        ///     The url for a tenant resource 
        /// </summary>
        public static string MakeTenantUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(TenantRouteName, new {id = tenantId});
        }

        /// <summary>
        ///     The url for a collection resource for the list of users in the context of a tenant
        /// </summary>
        public static string MakeTenantUsersUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(TenantUsersRouteName, new {id = tenantId});
        }

        /// <summary>
        ///     The url for an edit form for a tenant resource 
        /// </summary>
        public static string MakeTenantEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        /// <summary>
        ///     The url for a create form ona collection resource for the list of tenants 
        /// </summary>
        public static string MakeTenantCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }
        
        /// <summary>
        ///     The url for a collection resource for the list of tenants, given a string search criteria
        /// </summary>
        /// <remarks>
        ///     From a disclosure point-of-view this list is not presented via the API to
        ///     non-administrators.
        /// </remarks>
        public static string MakeTenantsUri(this string searchCriteria, IUrlHelper url)
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
        public static string MakeTenantsUri(this IUrlHelper url)
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
        public static string MakeTenantSearchUri(this IUrlHelper url)
        {
            return url.Link(TenantSearchRouteName, new { });
        }

        /// <summary>
        ///     The url for the search form for locating a matched tenant for a user
        /// </summary>
        public static string MakeTenantsSearchFormUri(this IUrlHelper url)
        {
            return url.Link(TenantSearchFormRouteName, new { });
        }

    }
}