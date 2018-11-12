using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using Api.RepresentationExtensions;
using Api.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Marvin.Cache.Headers;
using SemanticLink;
using SemanticLink.AspNetCore;
using SemanticLink.Form;

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

        /// <summary>
        ///     A user resource that parents todo lists and allows access to authentication mechanisms
        /// </summary>
        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseUser(Permission.Get)]
        public async Task<UserRepresentation> Get(string id)
        {
            var userId = User.GetId();


            return (await _userStore
                    .Get(userId))
                .ThrowObjectNotFoundExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(Url);
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

        /// <summary>
        ///     A public stateless create form that is fully cacheable.
        /// </summary>
        [HttpGet("form/create", Name = UserUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToUserCreateFormRepresentation();
        }


        /// <summary>
        ///     Update a user resource
        /// </summary>
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
        [HttpGet("{id}/todo", Name = UserUriFactory.UserTodosRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.UserTodoCollection, Permission.Get)]
        public async Task<FeedRepresentation> GetUserTodo(string id)
        {
            return (await _todoStore
                    .GetByUser(id))
                .ToUserTodoListFeedRepresentation(id, Url);
        }

        /////////////////////////
        //
        // (tenant) Tenant collection on a user


        /// <summary>
        ///     Tenants available for a user
        /// </summary>
        [HttpGet("{id}/tenant", Name = UserUriFactory.UserTenantsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [AuthoriseUserTenantCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUserTenants(string id)
        {
            return (await _tenantStore.GetTenantsForUser(id))
                .ToTenantFeedRepresentation(id, Url);
        }

        /// <summary>
        ///     Tenant available for a user
        /// </summary>
        [HttpGet("{id}/tenant/{tenantId}", Name = UserUriFactory.UserTenantRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [AuthoriseTenant(Permission.Get)]
        public async Task<TenantRepresentation> GetUserTenant(string id, string tenantId)
        {
            return (await _tenantStore.Get(tenantId))
                .ToRepresentation(id, Url);
        }


        /// <summary>
        ///     Create a tenant in the context of a user
        /// </summary>
        [HttpPost("{id}/tenant", Name = UserUriFactory.UserTenantsRouteName)]
        [AuthoriseUserTenantCollection(Permission.Post)]
        public async Task<CreatedResult> CreateTenant([FromBody] TenantCreateDataRepresentation data, string id)
        {
            (await _tenantStore.GetByCode(data.Code))
                .ThrowInvalidDataExceptionIfNotNull("Invalid tenant"); // already exists

            var ownerId = User.GetId();

            var tenantId = await _tenantStore.Create(
                ownerId,
                TrustDefaults.KnownHomeResourceId,
                data
                    .ThrowInvalidDataExceptionIfNull("Invalid tenant create data")
                    .FromRepresentation(),
                Permission.FullControl | Permission.Owner,
                CallerCollectionRights.Tenant);


            //////////////////////////
            // Add user to tenant
            // ==================
            //

            await _tenantStore.IncludeUser(
                tenantId,
                ownerId,
                Permission.Get | Permission.Owner,
                CallerCollectionRights.Tenant);

            return ownerId
                .MakeUserTenantUri(tenantId, Url)
                .MakeCreated();
        }

        /// <summary>
        ///     Todo list collection available for the user on a tenant
        /// </summary>
        [HttpGet("{id}/tenant/{tenantId}/todo", Name = UserUriFactory.UserTenantTodoRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [AuthoriseTenantTodoCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUserTenantTodos(string id, string tenantId)
        {
            return (await _todoStore.GetByTenantAndUser(tenantId, User.GetId()))
                .ToTenantFeedRepresentation(id, tenantId, Url);
        }

        /// <summary>
        ///     Create a todo list on a tenant in the context of a user
        /// </summary>
        /// <seealso cref="TodoController.CreateTodo"/>
        [HttpPost("{id}/tenant/{tenantId}/todo", Name = UserUriFactory.UserTenantTodoRouteName)]
        [AuthoriseTenantTodoCollection(Permission.Post)]
        public async Task<CreatedResult> CreateTodo(
            [FromBody] TodoCreateDataRepresentation data,
            string id,
            string tenantId)
        {
            var userId = User.GetId() ?? id;

            return (await _todoStore.Create(
                    userId,
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo list create data")
                        .FromRepresentation(tenantId, TodoType.List),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoUri(Url)
                .MakeCreated();
        }
    }
}