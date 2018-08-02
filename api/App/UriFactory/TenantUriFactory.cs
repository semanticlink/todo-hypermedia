using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class TenantUriFactory
    {
        public const string TenantRouteName = "Tenant";
        public const string SelfRouteName = "Tenants";
        public const string EditFormRouteName = "EditFormTenant";
        public const string CreateFormRouteName = "CreateFormTenant";
        public const string TenantUsersRouteName = "TenantUsers";

        public static string MakeTenantUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(TenantRouteName, new {id = tenantId});
        }

        public static string MakeTenantsUri(this IUrlHelper url)
        {
            return url.Link(SelfRouteName, new { });
        }

        public static string MakeTenantUsersUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(TenantUsersRouteName, new {id = tenantId});
        }

        public static string MakeTenantEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        public static string MakeTenantCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }
    }
}