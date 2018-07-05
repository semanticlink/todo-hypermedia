using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Web
{
    /// <summary>
    /// <para>
    ///     A reverse uri to params via routename coverter.
    /// </para>
    /// <para>
    ///     Takes in an absolute uri, with the Named Route returns the named param in the route template (that attribtued)
    /// </para>
    /// <para>
    ///    Used as a reverse mapping for <see cref="IUrlHelper.Link"/>
    /// </para>
    /// </summary>
    /// <remarks>
    ///    Aplogies, I couldn't find the proper way to do this so rolled my own
    /// </remarks>
    public static class RouteMatcher
    {
        /// <summary>
        ///     Looks for a named route and returns its route template
        /// </summary>
        /// <example>
        ///    [HttpGet("{id}", "Index")]
        ///
        ///     RouteTemplate("Index", HttpContext) --> "/{id}"
        /// </example>
        public static string RouteTemplate(string routeName, HttpContext context)
        {
            var provider = context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            return provider.ActionDescriptors
                .Items
                .Where(item => item?.AttributeRouteInfo?.Name == routeName)
                .Select(item => item?.AttributeRouteInfo?.Template)
                .FirstOrDefault();
        }

        public static RouteValueDictionary RouteParams(
            string routeName,
            string uri,
            HttpContext context)
        {
            return Match(
                RouteTemplate(routeName, context),
                new Uri(uri).PathAndQuery);
        }


        public static string GetParam(
            string param,
            string routeName,
            string uri,
            HttpContext context)
        {
            return RouteParams(routeName, uri, context).TryGetValue(param, out var val)
                ? val.ToString()
                : string.Empty;
        }

        /// <summary>
        ///     From https://blog.markvincze.com/matching-route-templates-manually-in-asp-net-core/
        /// </summary>
        public static RouteValueDictionary Match(string routeTemplate, string requestPath)
        {
            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, GetDefaults(template));

            var values = new RouteValueDictionary();
            matcher.TryMatch(requestPath, values);

            return values;
        }

        /// <summary>
        /// <para>
        ///    Extracts the default argument values from the template.
        /// </para>
        ///     From https://blog.markvincze.com/matching-route-templates-manually-in-asp-net-core/
        /// </summary>
        private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}