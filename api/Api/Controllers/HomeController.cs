using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
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
        private readonly User _user;
        private readonly ITenantStore _tenantStore;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public HomeController(
            Version version,
            User user,
            ITenantStore tenantStore,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration
        )
        {
            _version = version;
            _user = user;
            _tenantStore = tenantStore;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("", Name = HomeUriFactory.SelfRouteName)]
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
        public async Task<FeedRepresentation> GetTenants([FromQuery(Name = "q")] string search = null)
        {
            var enumerable = (!string.IsNullOrWhiteSpace(search)
                //
                //  Regardless of whether the caller is authenticated or not, a query with a name
                //  will return a collection with zero or one items matched by tenant code.
                //
                ? (await _tenantStore.GetByCode(search)).ToEnumerable()
                //
                : _user != null
                    // If the user is authenticated, then return all tenants that the user has access to.
                    ? (await _tenantStore.GetTenantsForUser(_user.Id))

                    // The user is not authenticated and there is no query, so the caller gets no tenants.
                    : new Tenant[] { });
            return enumerable
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
        public SearchFormRepresentation GetTenantsSearchForm()
        {
            return new TenantRepresentation()
                .ToTenantSearchFormRepresentation(Url);
        }

        [HttpGet("authenticate", Name = HomeUriFactory.AuthenticateCollectionRouteName)]
        public FeedRepresentation GetAuthenticateCollection()
        {
            return new User()
                .ToAuthenticationCollectionRepresentation(Url);
        }

        /// <summary>
        ///     A simple login form resource.
        /// </summary>
        [HttpGet("authenticate/form/login", Name = HomeUriFactory.AuthenticateLoginFormRouteName)]
        public SearchFormRepresentation GetAuthenticateForm()
        {
            return new UserRepresentation()
                .ToAuthenticateLoginFormRepresentation(Url);
        }

        [HttpPost("authenticate", Name = HomeUriFactory.AuthenticateRouteName)]
        public async Task<object> Login([FromBody] UserCreateDataRepresentation model)
        {
            var result = _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            (await result)
                .Succeeded
                .ThrowInvalidDataExceptionIf(x => x.Equals(false), result.ToString());

            var user = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);

            /*
             * TODO: this should actually create a new resource and return its uri rather than just the token
             */
            return user.Id
                .MakeUserUri(Url)
                .MakeCreatedToken(_configuration.GenerateJwtToken(model.Email, user));
        }
    }
}