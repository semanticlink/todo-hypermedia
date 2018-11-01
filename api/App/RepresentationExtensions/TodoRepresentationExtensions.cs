using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Domain.Representation.Enum;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.LinkRelations;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class TodoRepresentationExtensions
    {
        /// <summary>
        ///     Feed representation of todos parented on a user
        /// </summary>
        public static FeedRepresentation ToUserTodoFeedRepresentation(
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
//                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = todos
                    .Select(t => t.MakeTodoFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        /// <summary>
        ///     Feed representation of todos parented on a named todo list
        /// </summary>
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
        ///     Feed reperesentation of todos parented on a tags
        /// </summary>
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
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref = "Domain.Representation.TodoCreateDataRepresentation" />
        public static CreateFormRepresentation ToTodoCreateFormRepresentation(this IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.Self),

                    // no submit to make it cacheable
                },
                Items = MakeFormItems()
            };
        }

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
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <remarks>
        ///     The edit form has no <see cref = "IanaLinkRelation.Up" /> link to the
        ///     main resource, thus allowing the edit form to be the same for all instances of
        ///     the resource and thus fully cacheable.
        /// </remarks>
        public static EditFormRepresentation ToTodoEditFormRepresentation(this IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    url.MakeTodoEditFormUri().MakeWebLink(IanaLinkRelation.Self),
                },
                Items = MakeFormItems()
            };
        }

        private static FormItemRepresentation[] MakeFormItems()
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The title of the page",
                    Required = true
                },
                new SelectFormItemRepresentation
                {
                    Name = "state",
                    Description = "A todo can only toggle between open and complete.",
                    Required = false,
                    Multiple = false,
                    Items = new SelectOptionItemRepresentation[]
                    {
                        new SelectOptionValueItemRepresentation
                        {
                            Type = FormType.Enum,
                            Description = "The todo has been completed",
                            Label = "Completed",
                            Value = TodoState.Complete,
                            Name = "completed",
                        },
                        new SelectOptionValueItemRepresentation
                        {
                            Type = FormType.Enum,
                            Description = "The todo has been opened",
                            Label = "Open",
                            Value = TodoState.Open,
                            Name = "open",
                        },
                    }
                },
                new DateTimeInputFormItemRepresentation
                {
                    Name = "due",
                    Description = "The UTC date the todo is due"
                },
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