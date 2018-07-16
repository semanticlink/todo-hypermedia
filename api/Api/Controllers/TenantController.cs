using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("tenant")]
    [Authorize]
    public class TenantController : Controller
    {
        private readonly ITenantStore _tenantStore;
        private readonly IUserStore _userStore;

        public TenantController(ITenantStore tenantStore, IUserStore userStore)
        {
            _tenantStore = tenantStore;
            _userStore = userStore;
        }

        /// <summary>
        ///   The basic information about a tenant. 
        /// </summary>
        /// <remarks>
        ///    There is a small level of disclosure here because we want to be able to allow anonymous users
        ///     to find this tenant and then be able to register against it.
        /// </remarks>
        [HttpGet("{id}", Name = TenantUriFactory.SelfRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AllowAnonymous]
        public async Task<TenantRepresentation> Get(string id)
        {
            return (await _tenantStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .ToRepresentation(Url);
        }

        /// <summary>
        ///     User collection on a tenant
        /// </summary>
        /// <remarks>
        ///    Authenticated users will get back a list of users. Authenticted but unregistered
        ///    users get back an empty collection. This allows us to parent the creation of new users off
        ///    the tenant and allow users who are unknown but authenticated to register themselves.
        /// </remarks>
        [HttpGet("{id}/user/", Name = TenantUriFactory.TenantUsersRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetUsers(string id)
        {
            return (await _userStore.IsRegistered(User.GetExternalId())
                    ? await _tenantStore.GetUsersByTenant(id)
                    : new List<string>())
                .ToRepresentation(id, Url);
        }

        [HttpGet("{id}/form/user/create", Name = UserUriFactory.RegisterCreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AllowAnonymous]
        public async Task<CreateFormRepresentation> RegisterUserCreateForm(string id)
        {
            return (await _tenantStore.Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .Id
                .ToRegisterUserCreateFormRepresentation(Url);
        }

        /// <summary>
        ///     Create a user on a tenant. Creating a user requires that the incoming request is authenticated
        ///     because we use external id as a foreign key on the user.  
        /// </summary>
        /// <remarks>
        ///    A user is already authenticated and thus merely created 
        /// </remarks>
        [HttpPost("{id}/user")]
        public async Task<object> RegisterUser([FromBody] UserCreateDataRepresentation model, string id)
        {
            (await _tenantStore.Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant");

            var externalId = User.GetExternalId();
            var user = await _userStore.GetByExternalId(externalId);
            var userId = user.IsNull()
                ? await _userStore.Create(externalId, model)
                : user.Id;

            await _tenantStore.AddUser(id, userId);

            // now, we have the identity user, link this into the new user
            return userId
                .MakeUserUri(Url)
                .MakeCreated();
        }
    }
}