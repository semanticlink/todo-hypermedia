using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("todos")]
    public class TodoListController : Controller
    {
        private readonly ITenantStore _tenantStore;
        private readonly ITodoStore _todoStore;
        private readonly ITodoListStore _todoListStore;

        public TodoListController(ITodoListStore todoListStore, ITenantStore tenantStore, ITodoStore todoStore)
        {
            _todoListStore = todoListStore;
            _tenantStore = tenantStore;
            _todoStore = todoStore;
        }

        /// <summary>
        ///     A named todo list
        /// </summary>
        /// <see cref="UserController.GetUserTodo"/> for the todo collection as they are parented on a user
        [HttpGet("{id}", Name = TodoListUriFactory.TodoListRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseTodo(Permission.Get)]
        public async Task<TodoListRepresentation> GetById(string id)
        {
            return (await _todoListStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("todo list not found")
                .ToRepresentation(User.GetId(), Url);
        }

        /// <summary>
        ///     Update a named todo list
        /// </summary>    
        /// <see cref="UserController.CreateTodoList"/> for creating a todo list as they are parented on a user
        [HttpPut("{id}", Name = TodoListUriFactory.TodoListRouteName)]
        [AuthoriseTodo(Permission.Put)]
        public async Task<NoContentResult> Update(string id, [FromBody] TodoListRepresentation item)
        {
            await _todoListStore.Update(id,
                todo =>
                {
                    todo.Name = item.Name
                        .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo list must have a name");
                });

            return NoContent();
        }

        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = TodoListUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public FormRepresentation GetEditForm()
        {
            return Url.ToTodoListEditFormRepresentation();
        }

        /// <summary>
        ///     A private create form because it contains by value tenants that todo list can belong to
        /// </summary>
        [HttpGet("form/create", Name = TodoListUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public async Task<CreateFormRepresentation> GetCreateForm()
        {
            var tenants = await _tenantStore.GetTenantsForUser(User.GetId());
            return Url.ToTodoListCreateFormRepresentation(tenants);
        }

        [HttpDelete("{id}", Name = TodoListUriFactory.TodoListRouteName)]
        [AuthoriseTodo(Permission.Delete)]
        public async Task<NoContentResult> Delete(string id)
        {
            await _todoListStore.Delete(id);
            return NoContent();
        }
        
        ////////////////////////////////
        //
        // Todos on a list
        // ===============
        
        /// <summary>
        ///     User todo collection
        /// </summary>
        /// <see cref="TodoController.GetById"/>
        [HttpGet("{id}/todo", Name = TodoListUriFactory.TodoListTodosRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseTodo(Permission.Get)]
        public async Task<FeedRepresentation> GetTodos(string id)
        {
            var todos = await _todoStore
                .GetByParent(id);
            return todos
                .ToFeedRepresentation(id, Url);
        }

        [HttpPost("{id}/todo", Name = TodoListUriFactory.TodoListTodosRouteName)]
        [AuthoriseTodo(Permission.Post)]
        public async Task<CreatedResult> CreateTodo(string id, [FromBody] TodoCreateDataRepresentation data)
        {
            var userId = User.GetId();
                
            return (await _todoStore.Create(
                    userId,
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo create data")
                        .FromRepresentation(id),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

    }
}