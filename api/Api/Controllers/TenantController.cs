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
using Microsoft.Extensions.Logging;
using SemanticLink;
using SemanticLink.AspNetCore;
using SemanticLink.Form;
using Toolkit;

namespace Api.Controllers
{
    [Route("tenant")]
    public class TenantController : Controller
    {
        private ILogger<TenantController> Log { get; }
        private readonly ITenantStore _tenantStore;
        private readonly IUserStore _userStore;

        public TenantController(ITenantStore tenantStore, IUserStore userStore, ILogger<TenantController> log)
        {
            Log = log;
            _tenantStore = tenantStore;
            _userStore = userStore;
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
        [HttpGet("", Name = TenantUriFactory.TenantsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseMeAsap]
        public async Task<FeedRepresentation> GetTenantsWithOptionalSearch([FromQuery(Name = "q")] string search = null)
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
        ///   The basic information about a tenant. 
        /// </summary>
        /// <remarks>
        ///    There is a small level of disclosure here because we want authenticated users
        ///     to find this tenant and then be able to register against it.
        /// </remarks>
        [HttpGet("{id}", Name = TenantUriFactory.TenantRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.Tenant, Permission.Get)]
        public async Task<TenantRepresentation> Get(string id)
        {
            return (await _tenantStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .ToRepresentation(Url);
        }

        /// <summary>
        ///     A public stateless create form that is fully cacheable.
        /// </summary>
        [HttpGet("form/create", Name = TenantUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public CreateFormRepresentation TenantCreateForm()
        {
            return Url.ToTenantCreateFormRepresentation();
        }

        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = TenantUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public EditFormRepresentation TenantEditForm()
        {
            return Url.ToTenantEditFormRepresentation();
        }

        
        ///////////////////////////////////////
        //
        //  tenant search
        //  =============

        /// <summary>
        ///     Perform a search for a tenant. This is a highly constrained that will only disclose a single tenant
        ///     if the caller knows its code.
        /// </summary>
        /// <see cref="GetTenantsWithOptionalSearch"/>
        [HttpPost("search/", Name = TenantUriFactory.TenantSearchRouteName)]
        public IActionResult Search([FromBody] TenantSearchRepresentation criteria)
        {
            return criteria
                .ThrowInvalidDataExceptionIfNull("Invalid search form")
                .Search
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("Invalid tenant search name")
                .MakeTenantsUri(Url)
                .MakeCreated(Request, "Search resource created");
        }

        /// <summary>
        ///     A simple search form resource that is publicly cacheable
        /// </summary>
        [HttpGet("form/search", Name = TenantUriFactory.TenantSearchFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public SearchFormRepresentation GetTenantsSearchForm()
        {
            return new TenantRepresentation()
                .ToTenantSearchFormRepresentation(Url);
        }

        ///////////////////////////////////////
        //
        //  User collection
        //  ===============

        /// <summary>
        ///     User collection on a tenant
        /// </summary>
        /// <remarks>
        ///    Authenticated users will get back a list of users. Authenticated but unregistered
        ///    users get back an empty collection. This allows us to parent the creation of new users off
        ///    the tenant and allow users who are unknown but authenticated to register themselves.
        /// </remarks>
        [HttpGet("{id}/user/", Name = TenantUriFactory.TenantUsersRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseTenantUserCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUsers(string id)
        {
            return (await _tenantStore.GetUsersByTenant(id))
                .ToUserFeedRepresentation(id, Url);
        }

        /// <summary>
        ///     Create a user (if doesn't exist) and register for a tenant. 
        /// </summary>
        [HttpPost("{id}/user/")]
        [AuthoriseTenantUserCollection(Permission.Post)]
        public async Task<IActionResult> RegisterUser([FromBody] UserCreateDataRepresentation data, string id)
        {
            (await _tenantStore.Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant");

            var user = await _userStore.GetByExternalId(data.ExternalId);

            // Create the user if it doesn't already exist
            if (user.IsNull() || user.Id.IsNullOrWhitespace())
            {
                // make the user
                var userId = await _userStore.Create(
                    User.GetId(),
                    id,
                    data.FromRepresentation(),
                    Permission.FullControl,
                    CallerCollectionRights.User);

                // and stick it on the tenant
                await _tenantStore.IncludeUser(id, userId, Permission.Get, CallerCollectionRights.Tenant);

                Log.DebugFormat("New user {0} registered on tenant {1}", userId, id);

                // now, we have the identity user, link this into the new user
                return userId
                    .MakeUserUri(Url)
                    .MakeCreated();
            }

            // 409 if already on the tenant
            (await _tenantStore.IsRegisteredOnTenant(id, user.Id))
                .ThrowInvalidOperationExceptionIf(exists => exists, "User already exists on tenant");

            // 202 if now included
            await _tenantStore.IncludeUser(id, user.Id, Permission.Get, CallerCollectionRights.Tenant);

            Log.DebugFormat("Registered existing user {0} on tenant {1}", user.Id, id);

            return user.Id
                .MakeUserUri(Url)
                .MakeAccepted();
        }
    }
}