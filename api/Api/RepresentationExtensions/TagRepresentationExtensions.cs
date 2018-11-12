using System.Collections.Generic;
using System.Linq;
using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using Toolkit;

namespace Api.RepresentationExtensions
{
    public static class TagRepresentationExtensions
    {
        /// <summary>
        ///     A tag representation
        /// </summary>
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

                    // todos that contain this tag
                    tag.Id.MakeTagTodoCollectionUri(url).MakeWebLink(CustomLinkRelation.Tags),
                },

                Name = tag.Name,
            };
        }

        /// <summary>
        ///     A tag represenation in the context of a todo representation
        /// </summary>
        public static TagRepresentation ToTodoRepresentation(this Tag tag, string todoId, IUrlHelper url)
        {
            return new TagRepresentation
            {
                Links = new[]
                {
                    // self
                    tag.Id.MakeTodoTagUri(todoId, url).MakeWebLink(IanaLinkRelation.Self),

                    // up to the containing todo tag collection
                    todoId.MakeTodoTagCollectionUri(url).MakeWebLink(IanaLinkRelation.Up),

                    //canonical
                    tag.Id.MakeTagUri(url).MakeWebLink(IanaLinkRelation.Canonical),
                },

                Name = tag.Name
            };
        }


        /// <summary>
        ///     Reverse map with validation across-the-wire representation into in-memory representation
        /// </summary>
        public static TagCreateData FromRepresentation(this TagCreateDataRepresentation todo)
        {
            return new TagCreateData
            {
                Name = todo.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A tag requires a name"),
            };
        }

        /// <summary>
        ///     Feed representation of tags (at the root level)
        /// </summary>
        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<Tag> tags, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeAllTagsCollectionUri().MakeWebLink(IanaLinkRelation.Self),

                    // up link to todo collection
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // no create form because this is readonly collection
                },
                Items = tags
                    .Select(t => t.MakeFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        /// <summary>
        ///     Feed representation of tags in the context of a todo representation.
        /// </summary>
        /// <remarks>
        ///    This representation links to PUT and PATCH operations using <see cref="MediaType.UriList"/>
        ///     and <see cref="MediaType.JsonPatch"/> respectfully.
        /// </remarks>
        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<Tag> tags, string todoId, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    todoId.MakeTodoTagCollectionUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to the referring todo
                    todoId.MakeTodoUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // no create form because this is readonly collection
                    todoId.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.CreateForm),

                    // create form because for text/uri-list
                    todoId
                        .MakeTagEditFormUriListUri(url)
                        .MakeWebLink(IanaLinkRelation.EditForm, type: MediaType.UriList),

                    // create form because for text/uri-list
                    todoId
                        .MakeTagEditFormJsonPatchUri(url)
                        .MakeWebLink(IanaLinkRelation.EditForm, type: MediaType.JsonPatch)
                },
                Items = tags
                    .Select(t => t.MakeTodoFeedItemRepresentation(todoId, url))
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
    }
}