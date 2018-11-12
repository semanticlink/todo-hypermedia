using Api.RepresentationExtensions;
using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SemanticLink;
using SemanticLink.AspNetCore;
using Toolkit;
using CacheDuration = SemanticLink.AspNetCore.CacheDuration;

namespace Api.Controllers
{
    [Route("authenticate")]
    [AllowAnonymous]
    public class AuthenticateController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthenticateController(IConfiguration configuration
        )
        {
            _configuration = configuration;
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The authentication resources
        //  ============================
        //
        //  Currently supporting:
        //    - auth0 external authentication
        //

        /// <summary>
        ///     The logical home for all authenticators configuration for the clients. Each of the authenticators
        ///     available exist as link relations.
        /// </summary>
        /// <remarks>
        ///     Currently, only coded for 'Auth0' and needs to be generalised in the future.
        /// </remarks>
        [HttpGet("", Name = AuthenticateUriFactory.DefaultRoute)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public AuthenticateRepresentation Index()
        {
            return CustomLinkRelation.Auth0
                .ToAuthenticateRepresentation(Url);
        }

        /// <summary>
        ///     The configuration for the clients to talk to the Auth0 service
        /// </summary>
        /// <remarks>
        ///    This code is a little too specialised but has hints of being able to become generalised to
        ///     multiple authenticators
        /// </remarks>
        [HttpGet("{authenticator}", Name = AuthenticateUriFactory.AuthenticatorRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private, MaxAge = CacheDuration.Long)]
        public LinkedRepresentation Auth0(string authenticator)
        {
            authenticator
                .ThrowArgumentExceptionIf(a => a != CustomLinkRelation.Auth0,
                    nameof(authenticator),
                    "Authenticator not found");

            return _configuration
                .GetSection(Auth0Configuration.SectionName)
                .Get<Auth0Configuration>()
                .ToAuthenticatorRepresentation(CustomLinkRelation.Auth0, Url);
        }

        /// <summary>
        ///     A virtual resource such that each user (based on the Id from the authenticator)
        ///     may have different forms of authenticator.
        /// </summary>
        /// <remarks>
        ///    This code only accepts one type of authenticator <see cref="CustomLinkRelation.Auth0"/> 
        /// </remarks>
        [HttpGet("{authenticatorId}/{authenticator}", Name = AuthenticateUriFactory.UserAuthenticatorRouteName)]
        public IActionResult UserAuthenticator(string authenticatorId, string authenticator)
        {
            authenticator.ThrowArgumentExceptionIf(
                a => a != CustomLinkRelation.Auth0,
                nameof(authenticator),
                "Authenticator not found");

            return CustomLinkRelation.Auth0
                .MakeAuthenticatorUri(Url)
                .MakeRedirect();
        }
    }
}