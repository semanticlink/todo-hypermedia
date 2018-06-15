using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
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
        private readonly ITodoStore _todoStore;


        public TodoController(ITodoStore todoStore)
        {
            _todoStore = todoStore;
        }

        [HttpGet("", Name = TodoUriFactory.SelfRouteName)]
        public async Task<FeedRepresentation> GetAll()
        {
            return (await _todoStore
                    .GetAll())
                .ToFeedRepresentation(Url);
        }

        [HttpGet("form/create", Name = TodoUriFactory.CreateFormRouteName)]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToTodoCreateFormRepresentation();
        }


        [HttpPost]
        public async Task<CreatedResult> Create([FromBody] TodoCreateDataRepresentation todo)
        {
            return (await _todoStore.Create(todo
                    .ThrowInvalidDataExceptionIfNull("Invalid create data")
                    .FromRepresentation(Url)))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

        [HttpGet("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<TodoRepresentation> GetById(string id)
        {
            return (await _todoStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("todo not found")
                .ToRepresentation(Url);
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
        public FormRepresentation GetEditForm()
        {
            return Url.ToTodoEditFormRepresentation();
        }

        [HttpDelete("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<IActionResult> Delete(string id)
        {
            await _todoStore.Delete(id);
            return NoContent();
        }

        /// Immutable collection for now
        [HttpGet("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        public FeedRepresentation GetTodoTags(string id)
        {
            // hardcoded tags for now
            var tags = new[] {"Work", "Personal", "Grocery List"}.Select(label => new Tag {Name = label});

            return tags
                .ToFeedRepresentation(id, Url);
        }

        [HttpPost("{id}/tag/", Name = TagUriFactory.TodoTagCreateRouteName)]
        public IActionResult CreateTag([FromBody] TagCreateDataRepresentation createData)
        {
            // okay so it doesn't but it pretends to for now
            return new Tag {Name = createData.Name}
                .MakeTodoTagUri(Url)
                .MakeCreated();
        }
        
        
        [HttpGet("{id}/form/create", Name = TagUriFactory.CreateFormRouteName)]
        [AllowAnonymous]
        public CreateFormRepresentation GetCreateForm(string id)
        {
            return id.ToTagCreateFormRepresentation(Url);
        }
    }
}