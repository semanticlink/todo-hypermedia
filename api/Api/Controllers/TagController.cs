using System.Threading.Tasks;
using App.RepresentationExtensions;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("tag")]
    [Authorize]
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
        ///     Tags that live across all tenants
        /// </summary>
        [HttpGet("", Name = TagUriFactory.AllTagsRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetAllTags()
        {
            return (await _tagStore
                    .GetAll())
                .ToFeedRepresentation(Url);
        }

        [HttpGet("{id}", Name = TagUriFactory.TagRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<TagRepresentation> Get(string id)
        {
            return (await _tagStore
                    .Get(id))
                .ToRepresentation(Url);
        }

        [HttpGet("{id}/todo", Name = TagUriFactory.TagTodoCollectionRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetTagTodoCollection(string id)
        {
            return (await _todoStore
                    .GetByTag(id))
                .ToTodosOnTagFeedRepresentation(id, Url);
        }
    }
}