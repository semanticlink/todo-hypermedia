using System.Linq;
using App.RepresentationExtensions;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("tag")]
    [Authorize]
    public class TagController : Controller
    {
        /// Immutable collection for the all the todos
        [HttpGet("", Name = TagUriFactory.AllTagsRouteName)]
        public FeedRepresentation GetAllTags()
        {
            // hard coded, in practice these are harvested from all the todos
            var tags = new[] {"Work", "Personal", "Grocery List"}.Select(label => new Tag {Name = label});
            return tags
                .ToAllTagReadOnlyFeedRepresentation(Url);
        }

        [HttpGet("{label}", Name = TagUriFactory.TagRouteName)]
        public TagRepresentation Get(string label)
        {
            return new Tag {Name = label}.ToRepresentation(Url);
        }

    }
}