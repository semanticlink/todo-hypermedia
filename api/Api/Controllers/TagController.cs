using System.Threading.Tasks;
using Api.Authorisation;
using Api.RepresentationExtensions;
using Api.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;

namespace Api.Controllers
{
    [Route("tag")]
    public class TagController : Controller
    {
        private readonly ITagStore _tagStore;
        private readonly ITodoStore _todoStore;

        public TagController(ITagStore tagStore, ITodoStore todoStore)
        {
            _tagStore = tagStore;
            _todoStore = todoStore;
        }

        /// <summary>
        ///     Tags that live across all tenants for users that are authorised to read root tag collections
        /// </summary>
        /// <remarks>
        ///    This collection is available to tenants (by design) so that we get reuse.
        ///    Of course, this could be implemented quite differently.
        /// </remarks>
        [HttpGet("", Name = TagUriFactory.AllTagsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.RootTagCollection, Permission.Get, ResourceKey.Root)]
        public async Task<FeedRepresentation> GetAllTags()
        {
            return (await _tagStore
                    .GetAll())
                .ToFeedRepresentation(Url);
        }

        /// <summary>
        ///     A tag resource
        /// </summary>
        [HttpGet("{id}", Name = TagUriFactory.TagRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.Tag, Permission.Get)]
        public async Task<TagRepresentation> Get(string id)
        {
            return (await _tagStore
                    .Get(id))
                .ToRepresentation(Url);
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of todo resource
        //  ===============================
        //

        /// <summary>
        ///     A collection of todos that have a tag on them
        /// </summary>
        /// <remarks>
        ///    This collection has a disclosure issue that you can see the total number of todos across tenants
        ///    that have a tag. Access to the todo itself will be unauthorised. 
        /// </remarks>
        [HttpGet("{id}/todo", Name = TagUriFactory.TagTodoCollectionRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.TagTodoCollection, Permission.Get)]
        public async Task<FeedRepresentation> GetTagTodoCollection(string id)
        {
            return (await _todoStore
                    .GetByTag(id))
                .ToTodosOnTagFeedRepresentation(id, Url);
        }
    }
}