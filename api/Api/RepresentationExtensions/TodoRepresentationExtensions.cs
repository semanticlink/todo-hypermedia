using System.Collections.Generic;
using System.Linq;
using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Domain.Representation.Enum;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using Toolkit;

namespace Api.RepresentationExtensions
{
    public static class TodoRepresentationExtensions
    {
        /// <summary>
        ///     Feed representation of todo list collection parented on a user
        /// </summary>
        /// <remarks>
        ///    Todo lists are parented on a tenant, we aren't going to offer to create here (because we don't have
        ///     a tenant, although we could)
        /// </remarks>
        public static FeedRepresentation ToUserTodoListFeedRepresentation(
            this IEnumerable<Todo> todos,
            string todoListId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self (the feed of todos on a todo list)
                    todoListId.MakeUserTodosUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to a named todos
                    todoListId.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // create-form - you  must create on user tenant
                },
                Items = todos
                    .Select(t => t.MakeTodoFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        /// <summary>
        ///     Feed representation of todo items collection parented on a named todo list
        /// </summary>
        /// <see cref="ToUserTodoListFeedRepresentation"/>
        public static FeedRepresentation ToFeedRepresentation(
            this IEnumerable<Todo> todos,
            string todoListId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self (the feed of todos on a todo list)
                    todoListId.MakeTodoTodoListUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to a named todos
                    todoListId.MakeTodoUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // create-form - you  must create on user tenant
                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = todos
                    .Select(t => t.MakeTodoFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        /// <summary>
        ///     Feed representation of todos parented on a tags
        /// </summary>
        /// <remarks>
        ///    While the <see cref="Todo"/>s could be lists (because they could be) in practice <see cref="Tag"/>s
        ///    live on a todo item
        /// </remarks>
        public static FeedRepresentation ToTodosOnTagFeedRepresentation(
            this IEnumerable<Todo> todos,
            string tagId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    tagId.MakeTagTodoCollectionUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to user
                    tagId.MakeTagUri(url).MakeWebLink(IanaLinkRelation.Up),
                },
                Items = todos
                    .Select(t => t.MakeTodoFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        private static FeedItemRepresentation MakeTodoFeedItemRepresentation(this Todo todo, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = todo.Id.MakeTodoUri(url),
                Title = todo.Name,
            };
        }


        /// <summary>
        ///     Reverse map with validation across-the-wire representation into in-memory representation
        /// </summary>
        public static TodoCreateData FromRepresentation(
            this TodoCreateDataRepresentation todo,
            string parentId,
            TodoType type)
        {
            return new TodoCreateData
            {
                Name = todo.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo requires a name"),

                Parent = parentId
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo requires a tenant"),
                
                Type = type,
                Due = todo.Due,
                State = todo.State
            };
        }

        /// <summary>
        ///    A todo representation in the context of a user         
        /// </summary>
        /// <remarks>
        ///    The <see cref="TodoRepresentation"/> is either a todo list or a todo item
        /// </remarks>
        public static TodoRepresentation ToRepresentation(this Todo todo, string userId, IUrlHelper url)
        {
            // up is dependent on context of whether it is a list or an item
            var up = todo.Type == TodoType.Item
                ? todo.Parent.MakeTodoTodoListUri(url)
                : userId.MakeUserTodosUri(url);

            return new TodoRepresentation
            {
                Links = new[]
                {
                    // self
                    todo.Id.MakeTodoUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up
                    up.MakeWebLink(IanaLinkRelation.Up),

                    todo.Id.MakeTodoTodoListUri(url).MakeWebLink(CustomLinkRelation.Todos),

                    // the collection of todos tags (this may or may not have tags ie is an empty collection)
                    todo.Id.MakeTodoTagCollectionUri(url).MakeWebLink(CustomLinkRelation.Tags),

                    // edit-form
                    url.MakeTodoEditFormUri().MakeWebLink(IanaLinkRelation.EditForm)
                },
                Name = todo.Name,
                Completed = todo.State == TodoState.Complete,
                State = todo.State,
                Due = todo.Due
            };
        }


        /// <summary>
        ///     Feed representation of named todos parented on a user and tenant
        /// </summary>
        /// <remarks>
        ///     TODO: a search mechanism could be written finding only todos list by tenant
        ///     TODO: a create mechanism will need to take into account on what tenant it is being created
        /// </remarks>
        public static FeedRepresentation ToTenantFeedRepresentation(
            this IEnumerable<Todo> todos,
            string userId,
            string tenantId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    userId.MakeUserTenantTodoListUri(tenantId, url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to user
                    userId.MakeUserTenantUri(tenantId, url).MakeWebLink(IanaLinkRelation.Up),

                    // create-form for making a named todo list
                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = todos
                    .Select(t => t.MakeTodoListFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        private static FeedItemRepresentation MakeTodoListFeedItemRepresentation(this Todo todo, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = todo.Id.MakeTodoUri(url),
                Title = todo.Name
            };
        }
    }
}