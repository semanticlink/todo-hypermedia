using System.Linq;
using App.UriFactory;
using Domain.Models;
using Domain.Representation;
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
                    url.MakeAuthenticatePasswordUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // forms
                    url.MakeAuthenticateLoginFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },

                Items = new FeedItemRepresentation[0]
                    .ToArray()
            };
        }

        public static Auth0Representation ToRepresentation(this Auth0Configuration auth0Representation, IUrlHelper url)
        {
            return new Auth0Representation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenticateJsonWebTokenUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),
                },

                Audience = auth0Representation.Audience,
                ClientId = auth0Representation.ClientId,
                Domain = auth0Representation.Domain,
                Leeway = auth0Representation.Leeway,
                RequestedScopes = auth0Representation.RequestedScopes,
                ResponseType = auth0Representation.ResponseType
            };
        }
    }
}