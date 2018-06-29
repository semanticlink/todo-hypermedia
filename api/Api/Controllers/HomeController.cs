using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly Version _version;
        private readonly ITenantStore _tenantStore;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public HomeController(
            Version version,
            ITenantStore tenantStore,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration
        )
        {
            _version = version;
            _tenantStore = tenantStore;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("", Name = HomeUriFactory.SelfRouteName)]
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
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<IActionResult> GetTenant(string tenantCode)
        {
            return (await _tenantStore
                    .GetByCode(tenantCode))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .Id
                .MakeTenantUri(Url)
                .MakeRedirect();
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of tenant resource
        //  =================================
        //

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
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetTenants([FromQuery(Name = "q")] string search = null)
        {
            return (!string.IsNullOrWhiteSpace(search)
                    //
                    //  Regardless of whether the caller is authenticated or not, a query with a name
                    //  will return a collection with zero or one items matched by tenant code.
                    //
                    ? (await _tenantStore.GetByCode(search)).ToEnumerable()
                    //
                    : User != null
                        // If the user is authenticated, then return all tenants that the user has access to.
                        ? await _tenantStore.GetTenantsForUser(User.GetId())

                        // The user is not authenticated and there is no query, so the caller gets no tenants.
                        : new Tenant[] { })
                .ToRepresentation(search, Url);
        }


        /// <summary>
        ///     Perform a search for a tenant. This is a highly constrainted that will only disclose a single tenant
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
        public SearchFormRepresentation GetTenantsSearchForm()
        {
            return new TenantRepresentation()
                .ToTenantSearchFormRepresentation(Url);
        }

        [HttpGet("authenticate/login", Name = HomeUriFactory.AuthenticateLoginCollectionRouteName)]
        public FeedRepresentation GetAuthenticateCollection()
        {
            return Url.ToAuthenticationCollectionRepresentation();
        }

 
        [HttpGet("authenticate/bearer", Name = HomeUriFactory.AuthenticateBearerCollectionRouteName)]
        public FeedRepresentation GetBearerAuthenticateCollection()
        {
            return Url.ToAuthenticationCollectionRepresentation();
        }

        /// <summary>
        ///     A simple login form resource.
        /// </summary>
        [HttpGet("authenticate/form/login", Name = HomeUriFactory.AuthenticateLoginFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public SearchFormRepresentation GetAuthenticateForm()
        {
            return new UserRepresentation()
                .ToAuthenticateLoginFormRepresentation(Url);
        }

        /// <summary>
        ///     A simple bearer form resource.
        /// </summary>
        [HttpGet("authenticate/form/auth0", Name = HomeUriFactory.AuthenticateBearerFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public SearchFormRepresentation GetAuthenticateBearerForm()
        {
            return new UserRepresentation()
                .ToAuthenticateBearerFormRepresentation(Url);
        }

        [HttpPost("authenticate/login", Name = HomeUriFactory.AuthenticateUsernamePasswordRouteName)]
        public async Task<object> Login([FromBody] UserCreateDataRepresentation model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            result
                .Succeeded
                .ThrowInvalidDataExceptionIf(x => x.Equals(false), result.ToString());

            var user = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);

            /*
             * TODO: this should actually create a new resource and return its uri rather than just the token
             */
            return user
                .ThrowAccessDeniedExceptionIfNull("User creation denied")
                .Id
                .MakeUserUri(Url)
                .MakeCreatedToken(_configuration.GenerateJwtToken(user.Id, model.Email, user.Id));
        }
        
        [HttpPost("authenticate/bearer", Name = HomeUriFactory.AuthenticateBearerRouteName)]
        public IActionResult Login([FromBody] UserBearerCreateDataRepresentation model)
        {
           // decode Auth0 token
            
            // signin?    
            
            var fromDecodedToken = "from decoded token";
            var identityUser = _userManager.Users.SingleOrDefault(r => r.Email == fromDecodedToken);

            /*
             * TODO: this should actually create a new resource and return its uri rather than just the token
             */
            return /*user
                .ThrowAccessDeniedExceptionIfNull("User authentication denied")
                .Id
                .MakeUserUri(Url)*/
                ""
                .MakeCreatedToken(_configuration.GenerateJwtToken("dfdf", fromDecodedToken, "ddf"));
        }
    }
}