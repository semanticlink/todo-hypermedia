using System;
using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using Api.RepresentationExtensions;
using Api.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using SemanticLink.AspNetCore;
using Toolkit;

namespace Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly Version _version;
        private readonly ITenantStore _tenantStore;
        private readonly IUserStore _userStore;

        public HomeController(Version version, ITenantStore tenantStore, IUserStore userStore)
        {
            _version = version;
            _tenantStore = tenantStore;
            _userStore = userStore;
        }

        /// <summary>
        ///     The root/home of the API
        /// </summary>
        /// <remarks>
        ///    This is always unauthenticated and must not disclose information and **must** also be general enough to
        ///     everyone so that it can be publicly cacheable.
        /// </remarks>
        [HttpGet("", Name = HomeUriFactory.DefaultRoute)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public ApiRepresentation GetApi()
        {
            return new ApiVersion
                {
                    Version = _version.ToString()
                }
                .ToRepresentation(Url);
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of tenant resource
        //  =================================

        /// <summary>
        ///     Provides a redirect URL for locating a tenant without disclosing the name of any other tenants.
        /// </summary>
        [HttpGet(@"a/{tenantCode:regex(^[[\w\d\-\.]]+$)}")]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseRedirect]
        public async Task<IActionResult> GetTenant(string tenantCode)
        {
            return (await _tenantStore
                    .GetByCode(tenantCode))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .Id
                .MakeTenantForUserUri(User.GetId(), Url)
                .MakeRedirect();
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of user resource
        //  =================================
        //

        /// <summary>
        ///     This is a logical resource which represents all users
        /// </summary>
        /// <remarks>
        ///     If the user is an administrator we could disclose the list of
        ///     all tenants. However for normal users we could disclose their
        ///     single tenant in the collection. For anonymous user the list **must**
        ///     be empty.
        /// </remarks>
        [HttpGet("user/", Name = HomeUriFactory.UsersRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseRootUserCollection(Permission.Get)]
        public IActionResult GetUsers()
        {
            return NoContent();
        }

        [HttpPost("user/", Name = HomeUriFactory.UsersRouteName)]
        [AuthoriseRootUserCollection(Permission.Post)]
        public async Task<IActionResult> RegisterUser([FromBody] UserCreateDataRepresentation data)
        {
            return (await _userStore.Create(
                    TrustDefaults.KnownRootIdentifier,
                    TrustDefaults.KnownHomeResourceId,
                    data.FromRepresentation(),
                    Permission.FullControl | Permission.Owner,
                    CallerCollectionRights.User
                ))
                .MakeUserUri(Url)
                .MakeCreated();
        }
    }
}