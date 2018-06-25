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
        private readonly ITenantStore _tenantRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public TenantController(ITenantStore tenantRepository, UserManager<IdentityUser> userManager)
        {
            _tenantRepository = tenantRepository;
            _userManager = userManager;
        }

        [HttpGet("{id}", Name = TenantUriFactory.SelfRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<TenantRepresentation> Get(string id)
        {
            return (await _tenantRepository
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
        public async Task<FeedRepresentation> GetUsers(string id)
        {
            return (await _tenantRepository
                    .GetUsersByTenant(id))
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

            // now, we have the identity user, link this into the new user
            return user
                .Id
                .MakeUserUri(Url)
                .MakeCreated();
        }
    }
}