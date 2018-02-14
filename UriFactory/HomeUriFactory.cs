using Microsoft.AspNetCore.Mvc;

namespace TodoApi.UriFactory
{
    public static class HomeUriFactory
    {
        public const string SelfRouteName = "HomeRouteName";

        public static string MakeApiUri(this IUrlHelper url)
        {
            return url.Link(SelfRouteName, new { });
        }
    }
}