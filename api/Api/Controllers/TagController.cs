using System.Threading.Tasks;
using Api.Authorisation;
using App.RepresentationExtensions;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Toolkit.Representation.LinkedRepresentation;

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
        ///     Tags that live across all tenants
        /// </summary>
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