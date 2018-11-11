using System;
using JetBrains.Annotations;

namespace SemanticLink
{
    /// <summary>
    /// </summary>
    /// <seealso cref="IanaLinkRelation" />
    public static class WebLinkExtensions
    {
        public static WebLink MakeWebLink(
            this string url,
            [ValueProvider("Domain.ApiRepresentaion.Relations.IanaLinkRelation")]
            [ValueProvider("Domain.ApiRepresentaion.Relations.CustomLinkRelation")]
            string linkRelation,
            string title = null,
            string type = null)
        {
            return !string.IsNullOrWhiteSpace(url)
                ? new WebLink
                {
                    Title = title,
                    Rel = linkRelation,
                    HRef = url,
                    Type = type
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