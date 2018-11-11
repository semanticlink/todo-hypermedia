using System.Collections.Generic;
using System.Linq;
using Domain.Representation;
using Microsoft.AspNetCore.Http;
using SemanticLink;
using Toolkit;

namespace Api.Web
{
    public static class PatchRepresentationExtensions
    {
        public static PatchFeedRepresentation FromFeedRepresentation(this FeedRepresentation feed)
        {
            return new PatchFeedRepresentation
            {
                Items = feed.Items
                    .Select(item => new PatchFeedItemRepresentation
                    {
                        Id = item.Id,
                        Title = item.Title
                    })
                    .ToList()
            };
        }

        /// <summary>
        ///    Helper method to loop through all the <see cref="PatchFeedRepresentation.Items"/> (which have uris) and returns a list of ids
        /// </summary>
        /// <remarks>
        ///    It normalises IDs by searching through the provided list of Routes to grok the Id from.
        /// </remarks>
        public static List<string> ToTags(
            this PatchFeedRepresentation feed,
            IEnumerable<RouteAndParam> routeAndParams,
            HttpContext context)
        {
            return feed.Items
                .Select(item => routeAndParams
                    .Select(routeAndParam =>
                        item.Id.GetParamFromNamedRoute(routeAndParam.Param, routeAndParam.Route, context))
                    .First(tagid => !tagid.IsNullOrWhitespace()))
                .Distinct()
                .ToList();
        }
    }
}