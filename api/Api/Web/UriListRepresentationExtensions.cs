using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Toolkit;

namespace Api.Web
{
    public static class UriListRepresentationExtensions
    {
        /// <summary>
        ///    Helper method to loop through all the uriList and returns a list of ids
        /// </summary>
        /// <remarks>
        ///    It normalises IDs by searching through the provided list of Routes to grok the Id from.
        /// </remarks>
        public static List<string> ToTags(
            this string[] uriList,
            IEnumerable<RouteAndParam> routeAndParams,
            HttpContext context)
        {
            return uriList
                .ToList()
                .Select(uri => routeAndParams.Select(routeAndParam =>
                        uri.GetParamFromNamedRoute(routeAndParam.Param, routeAndParam.Route, context))
                    .First(tagid => !tagid.IsNullOrWhitespace()))
                .Distinct()
                .ToList();
        }
    }
}