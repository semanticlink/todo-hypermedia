using System;
using JetBrains.Annotations;

namespace SemanticLink
{
    /// <seealso cref="IanaLinkRelation" />
    public static class WebLinkExtensions
    {
        public static WebLink MakeWebLink(
            this string url,
            [ValueProvider("SemanticLink.IanaLinkRelation")]
            [ValueProvider("Domain.LinkRelations.CustomLinkRelation")]
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
            [ValueProvider("SemanticLink.IanaLinkRelation")]
            [ValueProvider("Domain.LinkRelations.CustomLinkRelation")]
            string linkRelation)
        {
            return url?.ToString().MakeWebLink(linkRelation);
        }
    }
}