using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SemanticLink.AspNetCore
{
    public static class UrlExtensions
    {
        /// <summary>
        ///     Get param values from a uri based on a named route
        /// </summary>
        /// <remarks>
        ///    This is the reverse mapping of <see cref="IUrlHelper.Link"/>
        /// </remarks>
        /// <example>
        ///    [HttpGet("{id}/tag/{tagId}", "Index")]
        ///
        ///     "https://example.com/1/tag/2".GetParamFromNameRoute("tagId", "Index", HttpContext)
        ///
        ///    returns: "2"
        /// 
        /// </example>
        /// <param name="uri">Absolute uri that is <see cref="Uri"/> parseable</param>
        /// <param name="routeParam">Name of the argument in the route/action</param>
        /// <param name="routeName">Name of the route as specified in the controller</param>
        /// <param name="context">Static <see cref="HttpContext"/> found on the <see cref="ControllerBase"/></param>
        /// <returns>Value of the param as a string</returns>
        public static string GetParamFromNamedRoute(
            this string uri,
            string routeParam,
            string routeName,
            HttpContext context)
        {
            return RouteMatcher.GetParam(routeParam, routeName, uri, context);
        }
    }
}