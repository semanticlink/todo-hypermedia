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