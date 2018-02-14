using Microsoft.AspNetCore.Mvc;

namespace TodoApi.UriFactory
{
    public static class TenantUriFactory
    {
        public const string SelfRouteName = "Tenant";

        public static string MakeTenantUri(this long tenantId, IUrlHelper url)
        {
            return url.Link(SelfRouteName, new {id = tenantId});
        }
    }
}