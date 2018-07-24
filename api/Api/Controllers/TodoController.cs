using System.Collections.Generic;
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
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    /// <see cref="UserController.GetUserTodos"/> for the todo collection as they are parented on a user
    [Route("todo")]
    public class TodoController : Controller
    {
        private readonly ITagStore _tagStore;
        private readonly IUserStore _userStore;
        private readonly ITodoStore _todoStore;

        public TodoController(ITodoStore todoStore, ITagStore tagStore, IUserStore userStore)
        {
            _todoStore = todoStore;
            _tagStore = tagStore;
            _userStore = userStore;
        }

        [HttpGet("{id}", Name = TodoUriFactory.TodoRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AuthoriseTodo(Permission.Get)]
        public async Task<TodoRepresentation> GetById(string id)
        {
            return (await _todoStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("todo not found")
                .ToRepresentation(User.GetIdentityId(), Url);
        }

        [HttpPost]
        [AuthoriseUserTodoCollection(Permission.Post, ResourceKey.User)]
        public async Task<CreatedResult> Create([FromBody] TodoCreateDataRepresentation data)
        {
            var userId = User.GetIdentityId();
            return (await _todoStore.Create(
                    userId, 
                    userId, // context is the userId
                    data
                        .ThrowInvalidDataExceptionIfNull("Invalid todo create data")
                        .FromRepresentation(Url),
                    Permission.FullControl,
                    CallerCollectionRights.Todo
                ))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

        [HttpPut("{id}", Name = TodoUriFactory.TodoRouteName)]
        [AuthoriseTodo(Permission.Put)]
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
        [AuthoriseForm]
        public FormRepresentation GetEditForm()
        {
            return Url.ToTodoEditFormRepresentation();
        }

        [HttpGet("form/create", Name = TodoUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToTodoCreateFormRepresentation();
        }

        [HttpDelete("{id}", Name = TodoUriFactory.TodoRouteName)]
        [AuthoriseTodo(Permission.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            await _todoStore.Delete(id);
            return NoContent();
        }

        ////////////////////////////////////////////////
        // 
        //  The tags on the todo collection
        //  ===============================


        /// <summary>
        ///     Tag collection. The todo is the logical parent of a set of (global) tags.
        /// </summary>
        [HttpGet("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AuthoriseTodoTagCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetTodoTags(string id)
        {
            var todo = await _todoStore.Get(id);

            return (await _tagStore.Get(todo.Tags))
                .ToFeedRepresentation(id, Url);
        }

        /// <summary>
        ///     Patch the tag collection on todos
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Creating a patch can be useful for large collections and want to provide a difference set (patch).
        /// </para>
        /// <para>
        ///     A <see cref="FeedRepresentation"/> can't be patched using JSON Merge Patch specification because of how it
        ///     works with arrays in JSON (it is either a DELETE *the* collection or PUT on collection). We'll
        ///     need to use JSON Patch to show add or remove from collection.
        ///</para>
        /// <para>
        ///     For this collection, it is a collection of READONLY items (ie global tags). So we won't be updating details
        ///     on a resource.
        ///</para>
        /// <para>
        /// <b>Remove:</b> however, our <see cref="FeedItemRepresentation"/> needs to be able to remove items by <see cref="FeedItemRepresentation.Id"/>
        ///
        ///     In this case, we have two options based on JSON Pointer (rfc9601):
        ///
        ///    <li>implement array index '/item/0' (given that feed do not guarantee 'natural' order)</li>
        ///    <li>extended syntax (to be implemented)'/items[id="https://example.com/tag/xxxx"]'</li>
        /// Example
        ///    <code>
        ///       [
        ///          { "op": "remove", "path": "/items/0" },
        ///          { "op": "remove", "path": "/items[id='https://example.com/tag/xxxx']" },
        ///       ]
        ///     </code>
        /// </para>
        /// <para>
        ///    <b>Add:</b> is much easier but the syntax has gotchas
        ///
        ///     <li>Adding to a list include '-' </li>
        /// Example
        ///    <code>
        ///       [
        ///          { "op": "add", "path": "/items/-", "value": { id: "https://example.com/tag/yyyy" } }
        ///       ]
        ///     </code>
        /// </para>
        ///     <li>see https://tools.ietf.org/html/rfc7396 (JSON Merge Patch)</li>
        ///     <li>see https://tools.ietf.org/html/rfc6902 (JSON Patch)</li>
        ///     <li>see https://tools.ietf.org/html/rfc6901#section-7 (JSON Pointer)</li>
        ///     <li>good examples http://benfoster.io/blog/aspnet-core-json-patch-partial-api-updates</li>
        /// </remarks>
        [HttpPatch("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        [Consumes(MediaType.JsonPatch)]
        [AuthoriseTodoTagCollection(Permission.Patch)]
        public async Task<IActionResult> PatchTagCollection(
            string id,
            [FromBody] JsonPatchDocument<PatchFeedRepresentation> patch)
        {
            // A little jiggery-pokery to get around arrays not supported by JsonPatchDocument
            var document = (await GetTodoTags(id)).FromFeedRepresentation();

            patch.ApplyTo(document);

            // normaliese
            await _todoStore.Update(id, todo =>
            {
                // normalise tags back to a string list
                // tags can come in various routes
                var todoTags = document.ToTags(new List<RouteAndParam>
                    {
                        new RouteAndParam {Route = TagUriFactory.TodoTagRouteName, Param = "tagId"},
                        new RouteAndParam {Route = TagUriFactory.TagRouteName, Param = "id"},
                    },
                    HttpContext);
                todo.Tags = todoTags;
            });

            return NoContent();
        }

        /// <summary>
        ///     Update the tag collection on todos
        /// </summary>
        /// <remarks>
        /// <para>
        ///    Checks that each of the Uris exist in the global collection, if not err.
        ///    Then updates the Tags list to the provided uri-list.
        /// </para>
        /// <para>
        ///    There is a need to translate between the global tag collection and references to them on todo as tag collections
        /// </para>
        /// </remarks>
        /// <param name="id">Todo</param>
        /// <param name="uriList">A todo tag uri (not a global tag uri)</param>
        [HttpPut("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        [Consumes(MediaType.UriList)]
        [AuthoriseTodoTagCollection(Permission.Put)]
        public async Task<IActionResult> PutTagCollection(string id, [FromBody] string[] uriList)
        {
            // check that global tags exist in the todo set sent through as a uriList
            var tagIds = uriList.ToTags(new List<RouteAndParam>
                {
                    new RouteAndParam {Route = TagUriFactory.TodoTagRouteName, Param = "tagId"},
                    new RouteAndParam {Route = TagUriFactory.TagRouteName, Param = "id"},
                },
                HttpContext);
            
            await _todoStore.Update(id, todo => { todo.Tags = tagIds; });

            return NoContent();
        }

        /// <summary>
        ///     Include a global tag onto a todo.
        /// </summary>
        /// <remarks>
        ///    This is a two-step process. First add to the global collection (if it doesn't already exist)
        ///     and then include in the todo.
        /// </remarks>
        [HttpPost("{id}/tag/", Name = TagUriFactory.TodoTagCreateRouteName)]
        [AuthoriseTodoTagCollection(Permission.Post)]
        public async Task<CreatedResult> CreateTag([FromBody] TagCreateDataRepresentation tag, string id)
        {
            var tagId = await _tagStore.Create(
                User.GetIdentityId(),
                TrustDefaults.KnownHomeResourceId,
                tag.ThrowInvalidDataExceptionIfNull("Invalid tag create data").FromRepresentation(),
                Permission.Get,
                CallerCollectionRights.Tag);

            await _todoStore.AddTag(id, tagId);

            return tagId
                .MakeTodoTagUri(id, Url)
                .MakeCreated();
        }

        /// <summary>
        ///     Tag form
        /// </summary>
        [HttpGet("{id}/tag/form/create", Name = TagUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public CreateFormRepresentation GetCreateForm(string id)
        {
            return id.ToTagCreateFormRepresentation(Url);
        }

        /// <summary>
        ///     Tag form for <see cref="MediaType.UriList"/>
        /// </summary>
        [HttpGet("{id}/tag/uri-list/create", Name = TagUriFactory.CreateFormUriListRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public EditFormRepresentation GetCreateFormUriList(string id)
        {
            return id.ToTagEditFormUriRepresentation(Url);
        }

        /// <summary>
        ///     Tag form for <see cref="MediaType.JsonPatch"/>
        /// </summary>
        [HttpGet("{id}/tag/json-patch/create", Name = TagUriFactory.EditFormJsonPatchRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public EditFormRepresentation GetEditFormJsonPatch(string id)
        {
            return id.ToTagEditFormJsonPatchUri(Url);
        }

        /// <summary>
        ///     A tag on a todo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpGet("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AuthoriseTodo(Permission.Get)]
        [AuthoriseTag(Permission.Get, "tagId")]
        public async Task<TagRepresentation> Get(string id, string tagId)
        {
            (await _todoStore.GetByIdAndTag(id, tagId))
                .ThrowInvalidDataExceptionIfNull($"Todo with tag not found '{id}'");


            return (await _tagStore
                    .Get(tagId))
                .ToTodoRepresentation(id, Url);
        }

        /// <summary>
        ///     Remove a tag from a todo. This is not a delete. The tag still exists in the global collection of tags
        /// </summary>
        [HttpDelete("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        [AuthoriseTodoTagCollection(Permission.Patch, "tagId")]
        public async Task<IActionResult> DeleteTag(string id, string tagId)
        {
            await _todoStore.DeleteTag(id, tagId);
            return NoContent();
        }
    }
}