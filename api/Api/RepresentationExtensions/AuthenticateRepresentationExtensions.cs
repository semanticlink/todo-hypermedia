using System.Linq;
using Api.UriFactory;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using SemanticLink.AspNetCore;

namespace Api.RepresentationExtensions
{
    public static class AuthenticateRepresentationExtensions
    {
        /// <summary>
        ///     A singleton authenticator representation with links to the available authenticators
        /// </summary>
        /// <remarks>
        ///    Currently, this is single authenticator strategy. This could alternatively be a collection.
        /// </remarks>
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

        /// <summary>
        ///     A singleton configuration for a Auth0 configuration.
        /// </summary>
        /// <remarks>
        ///    This is a pretty simple (non-polymorphic) approach to be extended as needed. This should be publicly
        ///     cacheable
        /// </remarks>
        public static Auth0Representation ToAuthenticatorRepresentation(
            this Auth0Configuration configuration,
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

                Audience = configuration.Audience,
                ClientId = configuration.ClientId,
                Domain = configuration.Domain,
                Leeway = configuration.Leeway,
                RequestedScopes = configuration.RequestedScopes.Split(' ').ToList(),
                ResponseType = configuration.ResponseType.Split(' ').ToList(),
                Realm = AuthenticatorDefaults.AuthenticatorAuth0Realm
            };
        }
    }
}