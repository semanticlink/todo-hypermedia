using System.Collections.Generic;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class AuthenicateRepresentationExtensions
    {
        public static AuthenticateRepresentation ToAuthenticateRepresentation(this IUrlHelper url)
        {
            return new AuthenticateRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenicateUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    url.MakeAuthenicateAuth0Uri().MakeWebLink(CustomLinkRelation.Auth0)
                },
            };
        }

        public static Auth0Representation ToAuth0Representation(
            this Auth0Configuration auth0Representation,
            IUrlHelper url)
        {
            return new Auth0Representation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenicateAuth0Uri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent is authenticate
                    url.MakeAuthenicateUri().MakeWebLink(IanaLinkRelation.Up),
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