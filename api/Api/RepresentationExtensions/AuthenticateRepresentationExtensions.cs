using System.Linq;
using Api;
using Api.UriFactory;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using AuthenticatorDefaults = SemanticLink.AspNetCore.AuthenticatorDefaults;

namespace Api.RepresentationExtensions
{
    public static class AuthenicateRepresentationExtensions
    {
        public static AuthenticateRepresentation ToAuthenticateRepresentation(this string authenticator, IUrlHelper url)
        {
            return new AuthenticateRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAuthenicateUri().MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of authenticate is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // authenicator (note: future is an array of authenticators
                    authenticator.MakeAuthenicatorUri(url).MakeWebLink(authenticator)
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
                    authenticator.MakeAuthenicatorUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // logical parent is authenticate
                    url.MakeAuthenicateUri().MakeWebLink(IanaLinkRelation.Up),
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