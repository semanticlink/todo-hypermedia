using System;
using JetBrains.Annotations;
using TodoApi.LinkRelations;

namespace TodoApi.Representation.LinkedRepresentation
{
    /// <summary>
    /// </summary>
    /// <seealso cref = "IanaLinkRelation" />
    /// <seealso cref = "CustomLinkRelation" />
    public static class WebLinkExtensions
    {
        public static WebLink MakeWebLink(
            this string url,
            [ValueProvider("Domain.ApiRepresentaion.Relations.IanaLinkRelation")]
            [ValueProvider("Domain.ApiRepresentaion.Relations.CustomLinkRelation")]
            string linkRelation,
            string title = null)
        {

            return !string.IsNullOrWhiteSpace(url)
                ? new WebLink
                {
                    Title = title,
                    Rel = linkRelation,
                    HRef = url
                }
                : null;
        }

        public static WebLink MakeWebLink(
            this Uri url,
            [ValueProvider("Relations.IanaLinkRelation")]
            [ValueProvider("Relations.CustomLinkRelation")]
            [ValueProvider("IanaLinkRelation")]
            [ValueProvider("CustomLinkRelation")]
            string linkRelation)
        {
            return url?.ToString().MakeWebLink(linkRelation);
        }
    }
}