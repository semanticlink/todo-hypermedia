using System.Collections.Generic;
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
        private readonly ITodoListStore _todoListStore;
        private readonly ITenantStore _tenantStore;

        public UserController(IUserStore userStore, ITodoListStore todoListStore, ITenantStore tenantStore)
        {
            _userStore = userStore;
            _todoListStore = todoListStore;
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
            var tenants = await _tenantStore.GetTenantsForUser(userId);


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
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/create", Name = UserUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToUserCreateFormRepresentation();
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
        [HttpGet("{id}/todolist", Name = UserUriFactory.UserTodosRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
//        [AuthoriseUserTenantTodoCollection(Permission.Get)]
        [AuthoriseMeAsap]
        public async Task<FeedRepresentation> GetUserTodo(string id)
        {
            return (await _todoListStore
                    .GetByUser(id))
                .ToFeedRepresentation(id, Url);
        }

        /// <summary>
        ///     Create a user named todo list
        /// </summary>
        [HttpPost("{id}/todolist", Name = UserUriFactory.UserTodosRouteName)]
//        [AuthoriseUserTenantTodoCollection(Permission.Post)]
        [AuthoriseMeAsap]
        public async Task<CreatedResult> CreateTodoList([FromBody] TodoListCreateDataRepresentation data, string id)
        {
            var userId = User.GetId();

            // reverse map an absolute uri into a tenantId
            var tenantId = data.Tenant.GetParamFromNamedRoute("id", TenantUriFactory.TenantRouteName, HttpContext);

            return (await _todoListStore.Create(
                    userId,
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo list create data")
                        .FromRepresentation(tenantId),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoListUri(Url)
                .MakeCreated();
        }

        /////////////////////////
        //
        // (tenant) Tenant collection on a user


        /// <summary>
        ///     Tenants available for a user
        /// </summary>
        [HttpGet("{id}/tenant", Name = UserUriFactory.UserTenantsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [AuthoriseMeAsap]
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
        [AuthoriseMeAsap]
        public async Task<TenantRepresentation> GetUserTenant(string id, string tenantId)
        {
            return (await _tenantStore.Get(tenantId))
                .ToRepresentation(id, Url);
        }


        [HttpPost("{id}/tenant", Name = UserUriFactory.UserTenantsRouteName)]
//        [AuthoriseRootTenantCollection(Permission.Post)]
        [AuthoriseMeAsap]
        public async Task<CreatedResult> Create([FromBody] TenantCreateDataRepresentation data, string id)
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
        ///     Todos available for the user on a tenant
        /// </summary>
        [HttpGet("{id}/tenant/{tenantId}/todolist", Name = UserUriFactory.UserTenantTodoListRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
//        [AuthoriseRootTenantCollection(Permission.Post)]
        [AuthoriseMeAsap]
        public async Task<FeedRepresentation> GetUserTenantTodos(string id, string tenantId)
        {
            return (await _todoListStore.GetByTenantAndUser(tenantId, User.GetId()))
                .ToTenantFeedRepresentation(id, tenantId, Url);
        }

        /// <summary>
        ///     Create a user named todo list
        /// </summary>
        [HttpPost("{id}/tenant/{tenantId}/todolist", Name = UserUriFactory.UserTenantTodoListRouteName)]
//        [AuthoriseUserTenantTodoCollection(Permission.Post)]
        [AuthoriseMeAsap]
        public async Task<CreatedResult> CreateTodoList(
            [FromBody] TodoListCreateDataRepresentation data,
            string id,
            string tenantId)
        {
            var userId = User.GetId();

            // reverse map an absolute uri into a tenantId
            var tenantFromBody =
                data.Tenant.GetParamFromNamedRoute("id", TenantUriFactory.TenantRouteName, HttpContext);

            return (await _todoListStore.Create(
                    userId,
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo list create data")
                        .FromRepresentation(tenantFromBody.IsNullOrWhitespace() ? tenantId : tenantFromBody),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoListUri(Url)
                .MakeCreated();
        }
    }
}