using System.Threading.Tasks;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
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
    [Route("todo")]
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITagStore _tagStore;
        private readonly ITodoStore _todoStore;

        public TodoController(ITodoStore todoStore, ITagStore tagStore)
        {
            _todoStore = todoStore;
            _tagStore = tagStore;
        }

        [HttpGet("{id}", Name = TodoUriFactory.TodoRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<TodoRepresentation> GetById(string id)
        {
            return (await _todoStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("todo not found")
                .ToRepresentation(User.GetId(), Url);
        }

        [HttpPost]
        public async Task<CreatedResult> Create([FromBody] TodoCreateDataRepresentation todo)
        {
            return (await _todoStore.Create(todo
                    .ThrowInvalidDataExceptionIfNull("Invalid todo create data")
                    .FromRepresentation(Url)))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

        [HttpPut("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<IActionResult> Update(string id, [FromBody] TodoRepresentation item)
        {
            await _todoStore.Update(id,
                todo =>
                {
                    todo.Name = item.Name
                        .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo must have a name");

                    todo.State = item.State;
                    todo.Due = item.Due;
                });
            return NoContent();
        }

        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = TodoUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public FormRepresentation GetEditForm()
        {
            return Url.ToTodoEditFormRepresentation();
        }

        [HttpGet("form/create", Name = TodoUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToTodoCreateFormRepresentation();
        }

        [HttpDelete("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<IActionResult> Delete(string id)
        {
            await _todoStore.Delete(id);
            return NoContent();
        }

        ////////////////////////////////////////////////
        /// 
        //  The tags on the todo collection
        //  ===============================
        [HttpGet("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetTodoTags(string id)
        {
            var todo = await _todoStore.Get(id);

            return (await _tagStore.Get(todo.Tags))
                .ToFeedRepresentation(id, Url);
        }

        [HttpPost("{id}/tag/", Name = TagUriFactory.TodoTagCreateRouteName)]
        public async Task<CreatedResult> CreateTag([FromBody] TagCreateDataRepresentation tag, string id)
        {
            var tagId = await _tagStore.Create(tag
                .ThrowInvalidDataExceptionIfNull("Invalid tag create data")
                .FromRepresentation());

            await _todoStore.AddTag(id, tagId);

            return tagId
                .MakeTodoTagUri(id, Url)
                .MakeCreated();
        }

        [HttpGet("{id}/tag/form/create", Name = TagUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AllowAnonymous]
        public CreateFormRepresentation GetCreateForm(string id)
        {
            return id.ToTagCreateFormRepresentation(Url);
        }

        [HttpGet("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        public async Task<TagRepresentation> Get(string id, string tagId)
        {
            (await _todoStore.GetByIdAndTag(id, tagId))
                .ThrowInvalidDataExceptionIfNull($"Todo with tag not found '{id}'");


            return (await _tagStore
                    .Get(tagId))
                .ToTodoRepresentation(id, Url);
        }

        [HttpDelete("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        public async Task<IActionResult> DeleteTag(string id, string tagId)
        {
            await _todoStore.DeleteTag(id, tagId);
            return NoContent();
        }
    }
}