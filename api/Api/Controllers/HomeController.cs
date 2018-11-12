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
using SemanticLink;
using SemanticLink.AspNetCore;
using SemanticLink.Form;
using Toolkit;
using CacheDuration = SemanticLink.AspNetCore.CacheDuration;
using HomeUriFactory = Api.UriFactory.HomeUriFactory;
using TrustDefaults = Api.Web.TrustDefaults;

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

        /// <summary>
        ///     This is a logical resource which represents all tenants or a search for a single tenant (as a list)
        /// </summary>
        /// <remarks>
        ///     If the user is an administrator we could disclose the list of
        ///     all tenants. However for normal users we could disclose their
        ///     single tenant in the collection. For anonymous user the list **must**
        ///     be empty.
        /// </remarks>
        [HttpGet("tenant/", Name = HomeUriFactory.TenantsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseMeAsap]
        public async Task<FeedRepresentation> GetTenants([FromQuery(Name = "q")] string search = null)
        {
            return (!string.IsNullOrWhiteSpace(search)
                    //
                    //  Regardless of whether the caller is authenticated or not, a query with a name
                    //  will return a collection with zero or one items matched by tenant code.
                    //
                    ? (await _tenantStore.GetByCode(search)).ToEnumerable()
                    //
                    : User != null && User.Identity.IsAuthenticated
                        // If the user is authenticated, then return all tenants that the user has access to.
                        ? await _tenantStore.GetTenantsForUser(User.GetId())

                        // The user is not authenticated and there is no query, so the caller gets no tenants.
                        : new Tenant[] { })
                .ToSearchFeedRepresentation(User.GetId(), search, Url);
        }


        /// <summary>
        ///     Perform a search for a tenant. This is a highly constrained that will only disclose a single tenant
        ///     if the caller knows its code.
        /// </summary>
        [HttpPost("tenant/search/", Name = HomeUriFactory.HomeTenantSearchRouteName)]
        public IActionResult Search([FromBody] TeantSearchRepresentation criteria)
        {
            return criteria
                .ThrowInvalidDataExceptionIfNull("Invalid search form")
                .Search
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("Invalid tenant search name")
                .MakeHomeTenantsUri(Url)
                .MakeCreated(Request, "Search resource created");
        }

        /// <summary>
        ///     A simple search form resource.
        /// </summary>
        [HttpGet("tenant/form/search", Name = HomeUriFactory.HomeTenantSearchFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public SearchFormRepresentation GetTenantsSearchForm()
        {
            return new TenantRepresentation()
                .ToTenantSearchFormRepresentation(Url);
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