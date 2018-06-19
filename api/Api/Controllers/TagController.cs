using System.Threading.Tasks;
using App.RepresentationExtensions;
using Domain.Persistence;
using Domain.Representation;
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


        public TagController(ITagStore tagStore)
        {
            _tagStore = tagStore;
        }

        /// <summary>
        ///     Tags that live across all tenants
        /// </summary>
        /// <returns></returns>
        [HttpGet("", Name = TagUriFactory.AllTagsRouteName)]
        public async Task<FeedRepresentation> GetAllTags()
        {
            return (await _tagStore
                    .GetAll())
                .ToFeedRepresentation(Url);
        }

        [HttpGet("{id}", Name = TagUriFactory.TagRouteName)]
        public async Task<TagRepresentation> Get(string id)
        {
            return (await _tagStore
                    .Get(id))
                .ToRepresentation(Url);
        }
    }
}