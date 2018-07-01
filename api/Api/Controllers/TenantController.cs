using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("tenant")]
    [Authorize]
    public class TenantController : Controller
    {
        private readonly ITenantStore _tenantStore;
        private readonly UserManager<IdentityUser> _userManager;

        public TenantController(ITenantStore tenantStore, UserManager<IdentityUser> userManager)
        {
            _tenantStore = tenantStore;
            _userManager = userManager;
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
                .ToRepresentation(Url);
        }

        /// <summary>
        ///     User collection on a tenant
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/user/", Name = TenantUriFactory.TenantUsersRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AllowAnonymous]
        public async Task<FeedRepresentation> GetUsers(string id)
        {
            return (User.Identity.IsAuthenticated
                    ? await _tenantStore.GetUsersByTenant(id)
                    : new List<string>())
                .ToRepresentation(id, Url);
        }

        /// <summary>
        ///     Create a user on a tenant. Creating a user requires that it is registered in the identity provider. 
        /// </summary>
        /// <remarks>
        ///    A user cannot be created and logged in in the same transaction.
        /// </remarks>
        [HttpPost("{id}/user")]
        [AllowAnonymous] // this should be restricture to role/claim
        public async Task<object> CreateUser([FromBody] UserCreateDataRepresentation model, string id)
        {
            var user = new IdentityUser
            {
                UserName = model.Email.ThrowInvalidDataExceptionIfNullOrWhiteSpace("No email provided"),
                Email = model.Email
            };

            var result = _userManager.CreateAsync(user, model.Password);
            (await result)
                .Succeeded
                .ThrowInvalidDataExceptionIf(x => x.Equals(false) && !result.ToString().Contains("DuplicateUserName"),
                    result.ToString());

            await _tenantStore.AddUser(id, user.Id);

            // now, we have the identity user, link this into the new user
            return user
                .Id
                .MakeUserUri(Url)
                .MakeCreated();
        }
    }
}