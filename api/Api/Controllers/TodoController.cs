using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("todo")]
    public class TodoController : Controller
    {
        private readonly ITodoRepository _db;

        public TodoController(ITodoRepository db)
        {
            _db = db;
        }

        [HttpGet("", Name = TodoUriFactory.SelfRouteName)]
        public FeedRepresentation GetAll()
        {
            return _db
                .GetAll()
                .ToFeedRepresentation(Url);
        }

        [HttpGet("form/create", Name = TodoUriFactory.CreateFormRouteName)]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToTodoCreateFormRepresentation();
        }


        [HttpPost]
        public CreatedResult Create([FromBody] TodoCreateDataRepresentation todo)
        {
            return _db.Create(todo
                    .ThrowInvalidDataExceptionIfNull("Invalid create data")
                    .FromRepresentation(Url))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

        [HttpGet("{id}", Name = TodoUriFactory.TodoRouteName)]
        public TodoRepresentation GetById(string id)
        {
            return _db
                .Get(id)
                .ThrowObjectNotFoundExceptionIfNull("todo not found")
                .ToRepresentation(Url);
        }

        [HttpPut("{id}", Name = TodoUriFactory.TodoRouteName)]
        public IActionResult Update(string id, [FromBody] TodoRepresentation item)
        {
            _db.Update(id,
                todo =>
                {
                    todo.Name = item.Name
                        .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo must have a name");

                    todo.Completed = item.Completed;

                    todo.Due = item.Due;
                });
            return new NoContentResult();
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
        public IActionResult Delete(string id)
        {
            _db.Delete(id);
            return new NoContentResult();
        }

    }
}