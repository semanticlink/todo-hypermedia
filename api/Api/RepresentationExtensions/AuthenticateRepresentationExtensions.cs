using System.Linq;
using Api.UriFactory;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using AuthenticatorDefaults = SemanticLink.AspNetCore.AuthenticatorDefaults;

namespace Api.RepresentationExtensions
{
    public static class AuthenticateRepresentationExtensions
    {
        public static AuthenticateRepresentation ToAuthenticateRepresentation(this string authenticator, IUrlHelper url)
        {
            return new AuthenticateRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenticatorUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // authenticator (note: future is an array of authenticators
                    authenticator.MakeAuthenticatorUri(url).MakeWebLink(authenticator)
                },
            };
        }

        public static Auth0Representation ToAuthenticatorRepresentation(
            this Auth0Configuration auth0Representation,
            string authenticator,
            IUrlHelper url)
        {
            return new Auth0Representation
            {
                Links = new[]
                {
                    // self
                    authenticator.MakeAuthenticatorUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // logical parent is authenticate
                    url.MakeAuthenticatorUri().MakeWebLink(IanaLinkRelation.Up),
                },

                Audience = auth0Representation.Audience,
                ClientId = auth0Representation.ClientId,
                Domain = auth0Representation.Domain,
                Leeway = auth0Representation.Leeway,
                RequestedScopes = auth0Representation.RequestedScopes.Split(' ').ToList(),
                ResponseType = auth0Representation.ResponseType.Split(' ').ToList(),
                Realm = AuthenticatorDefaults.AuthenticatorAuth0Realm
            };
        }
    }
}