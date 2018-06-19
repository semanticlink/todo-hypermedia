using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.LinkRelations;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class TagsRepresentationExtensions
    {
        public static TagRepresentation ToRepresentation(this Tag tag, IUrlHelper url)
        {
            return new TagRepresentation
            {
                Links = new[]
                {
                    // self
                    tag.Id.MakeTagUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up
                    url.MakeAllTagsCollectionUri().MakeWebLink(IanaLinkRelation.Up),
                },

                Name = tag.Name,
                Count = tag.Count,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };
        }

        public static TagRepresentation ToTodoRepresentation(this Tag tag, string todoId, IUrlHelper url)
        {
            return new TagRepresentation
            {
                Links = new[]
                {
                    // self
                    tag.Id.MakeTodoTagUri(todoId, url).MakeWebLink(IanaLinkRelation.Self),

                    //canonical
                    tag.Id.MakeTagUri(url).MakeWebLink(IanaLinkRelation.Canonical),
                },

                Name = tag.Name
            };
        }

        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<Tag> tags, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAllTagsCollectionUri().MakeWebLink(IanaLinkRelation.Self),

                    // up link to todo collection
                    url.MakeTodoCollectionUri().MakeWebLink(IanaLinkRelation.Up),

                    // no create form because this is readonly collection
                },
                Items = tags
                    .Select(t => t.MakeFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<Tag> tags, string id, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    id.MakeTodoTagCollectionUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to the referring todo
                    id.MakeTodoUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // no create form because this is readonly collection
                    id.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = tags
                    .Select(t => t.MakeTodoFeedItemRepresentation(id, url))
                    .ToArray()
            };
        }

        private static FeedItemRepresentation MakeTodoFeedItemRepresentation(
            this Tag tag,
            string todoId,
            IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = tag.Id.MakeTodoTagUri(todoId, url),
                Title = tag.Name,
            };
        }

        private static FeedItemRepresentation MakeFeedItemRepresentation(
            this Tag tag,
            IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = tag.Id.MakeTagUri(url),
                Title = tag.Name,
            };
        }


        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref = "TagCreateDataRepresentation" />
        public static CreateFormRepresentation ToTagCreateFormRepresentation(this string id, IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    id.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // submit against the parent collection
                    id.MakeTodoTagCollectionUri(url).MakeWebLink(CustomLinkRelation.Submit),
                },
                Items = MakeCreateFormItems()
            };
        }

        public static TagCreateData FromRepresentation(this TagCreateDataRepresentation todo)
        {
            return new TagCreateData
            {
                Name = todo.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A tag requires a name"),
            };
        }

        private static FormItemRepresentation[] MakeCreateFormItems()
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "Tag (or category) name",
                    Required = true
                }
            };
        }
    }
}