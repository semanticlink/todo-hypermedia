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
        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<Todo> todos, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeTodoCollectionUri().MakeWebLink(IanaLinkRelation.Self),

                    // up link to root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),
                    
                    // all tags currently created on todos
                    url.MakeAllTagsCollectionUri().MakeWebLink(CustomLinkRelation.Tags),

                    // create-form
                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
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
        /// <seealso cref = "TodoCreateDataRepresentation" />
        public static CreateFormRepresentation ToTodoCreateFormRepresentation(this IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    url.MakeTodoCreateFormUri().MakeWebLink(IanaLinkRelation.Self),

                    // Create a new organisation on the collection
                    url.MakeTodoCollectionUri().MakeWebLink(CustomLinkRelation.Submit)
                },
                Items = MakeCreateFormItems()
            };
        }

        private static FormItemRepresentation[] MakeCreateFormItems()
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "What needs to be done?",
                    Required = true
                },
                new CheckInputFormItemRepresentation
                {
                    Name = "state",
                    Description = "A todo can only toggle between open and complete."
                },
                new DateTimeInputFormItemRepresentation {Name = "due", Description = "The UTC date the todo is due"},
            };
        }

        public static TodoCreateData FromRepresentation(this TodoCreateDataRepresentation todo, IUrlHelper url)
        {
            return new TodoCreateData
            {
                Name = todo.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo requires a name"),

                Due = todo.Due,

                State = todo.State
            };
        }

        public static TodoRepresentation ToRepresentation(this Todo todo, IUrlHelper url)
        {
            return new TodoRepresentation
            {
                Links = new[]
                {
                    // self
                    todo.Id.MakeTodoUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // the collection of todos is the logical parent
                    url.MakeTodoCollectionUri().MakeWebLink(IanaLinkRelation.Up),
                    
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
                Items = MakeEditFormItems()
            };
        }

        private static FormItemRepresentation[] MakeEditFormItems()
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
                new CollectionInputFormItemRepresentation
                {
                    Name = "tags",
                    Description = "A todo can be grouped by tags (known also as categories)",
                    Required = false,
                    Multiple = true
                }, 
            };
        }
    }
}