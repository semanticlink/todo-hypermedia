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
    }
}