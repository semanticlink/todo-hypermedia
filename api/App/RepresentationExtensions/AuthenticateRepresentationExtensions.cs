using System.Linq;
using App.UriFactory;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class AuthenicateRepresentationExtensions
    {
        public static FeedRepresentation ToAuthenticationCollectionRepresentation(this IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenticateBearerUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // forms
                    url.MakeAuthenticateBearerFormUri().MakeWebLink(IanaLinkRelation.CreateForm),
                    url.MakeAuthenticateLoginFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },

                Items = new FeedItemRepresentation[0]
                    .ToArray()
            };
        }
    }
}