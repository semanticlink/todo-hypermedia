using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class TenantUriFactory
    {
        public const string SelfRouteName = "Tenant";

        public static string MakeTenantUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(SelfRouteName, new {id = tenantId});
        }
    }
}