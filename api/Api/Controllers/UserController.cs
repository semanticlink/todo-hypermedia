using System.Linq;
using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;
using Marvin.Cache.Headers;

namespace Api.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IUserStore _userStore;
        private readonly ITodoStore _todoStore;
        private readonly ITenantStore _tenantStore;

        public UserController(IUserStore userStore, ITodoStore todoStore, ITenantStore tenantStore)
        {
            _userStore = userStore;
            _todoStore = todoStore;
            _tenantStore = tenantStore;
        }

        /// <summary>
        ///     A virtual resource to return a redirect. 
        /// </summary>
        /// <remarks>
        ///     However, redirects with preflight requests do not work in Firefox less than v63.  Firefox just marked their
        ///     issue "fixed" for Firefox 63, which is currently scheduled for beta on 2018-09-05
        ///     and stable on 2018-10-23.
        /// 
        ///     The test page is here, open on browsers to test functionality:
        /// 
        ///         http://storage.googleapis.com/shaka-demo-assets/_bugs/cors_redirect/index.html
        ///
        ///     The best page to track this issues is here: https://github.com/google/shaka-player/issues/666
        /// </remarks>
        [HttpGet("me", Name = UserUriFactory.UserMeName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseRedirect]
        public IActionResult Me()
        {
            return User
                .GetId()
                .MakeUserUri(Url)
                .MakeRedirect();
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseUser(Permission.Get)]
        public async Task<UserRepresentation> Get(string id)
        {
            var userId = User.GetId();
            var tenants = await _tenantStore.GetTenantsForUser(userId);


            return (await _userStore
                    .Get(userId))
                .ThrowObjectNotFoundExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(tenants.ToList(), Url);
        }


        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = UserUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public FormRepresentation GetEditForm()
        {
            return Url.ToUserEditFormRepresentation();
        }

        [HttpPut("{id}")]
        [AuthoriseUser(Permission.Put)]
        public async Task<IActionResult> UpdateUser([FromBody] UserEditData model, string id)
        {
            await _userStore.Update(id, user =>
            {
                user.Email = model.Email;
                user.Name = model.Name;
            });
            return NoContent();
        }

        /////////////////////////
        //
        // (tenant) Todo collection on a user

        /// <summary>
        ///     User todo collection
        /// </summary>
        /// <see cref="TodoController.GetById"/>
        [HttpGet("tenant/{tenantId}/todo", Name = UserUriFactory.UserTenantTodosRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseUserTenantTodoCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUserTodos(string tenantId)
        {
            return (await _todoStore
                    .GetByTenant(tenantId))
                .ToFeedRepresentation(User.GetId(), tenantId, Url);
        }

        [HttpPost("tenant/{tenantId}/todo", Name = UserUriFactory.UserTenantTodosRouteName)]
        [AuthoriseUserTenantTodoCollection(Permission.Post)]
        public async Task<CreatedResult> Create([FromBody] TodoCreateDataRepresentation data, string tenantId)
        {
            var userId = User.GetId();
            return (await _todoStore.Create(
                    userId,
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo create data")
                        .FromRepresentation(tenantId),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoUri(Url)
                .MakeCreated();
        }
    }
}