using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("authenticate")]
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
        ///     The configuration for the clients, all services available as link relations
        /// </summary>
        [HttpGet("", Name = AuthenticateUriFactory.DefaultRoute)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AllowAnonymous]
        public AuthenticateRepresentation Index()
        {
            return Url.ToAuthenticateRepresentation();
        }

        /// <summary>
        ///     The configuration for the clients to talk to the Auth0 service
        /// </summary>
        [HttpGet("auth0", Name = AuthenticateUriFactory.Auth0RouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private, MaxAge = CacheDuration.Long)]
        public Auth0Representation Auth0()
        {
            return _configuration
                .GetSection(Auth0Configuration.SectionName)
                .Get<Auth0Configuration>()
                .ToAuth0Representation(Url);
        }
    }
}