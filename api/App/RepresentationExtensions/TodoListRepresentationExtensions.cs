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
    public static class TodoListRepresentationExtensions
    {
        /// <summary>
        ///     Feed representation of named todos parented on a user
        /// </summary>
        /// <remarks>
        ///     TODO: a search mechanism could be written finding only todos list by tenant
        ///     TODO: a create mechanism will need to take into account on what tenant it is being created
        /// </remarks>
        public static FeedRepresentation ToFeedRepresentation(
            this IEnumerable<TodoList> todos,
            string userId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    userId.MakeUserTodosUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up link to user
                    userId.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // create-form for making a named todo list
                    url.MakeTodoListCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = todos
                    .Select(t => t.MakeTodoListFeedItemRepresentation(url))
                    .ToArray()
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
            this IEnumerable<TodoList> todos,
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
                    url.MakeTodoListCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = todos
                    .Select(t => t.MakeTodoListFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        /// <summary>
        ///     A named todo list with links to the tenant
        /// </summary>
        public static TodoListRepresentation ToRepresentation(this TodoList todoList, string userId, IUrlHelper url)
        {
            return new TodoListRepresentation
            {
                Links = new[]
                {
                    // self
                    todoList.Id.MakeTodoListUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // up - user as the logical parent
                    userId.MakeUserTodosUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // tenant - the tenant as (one) logical parent
                    userId.MakeUserTenantUri(todoList.Tenant, url).MakeWebLink(CustomLinkRelation.Tenant),

                    // todos
                    todoList.Id.MakeTodoListTodosUri(url).MakeWebLink(CustomLinkRelation.Todos),

                    // edit-form
                    url.MakeTodoListEditFormUri().MakeWebLink(IanaLinkRelation.EditForm)
                },
                Name = todoList.Name
            };
        }

        private static FeedItemRepresentation MakeTodoListFeedItemRepresentation(this TodoList todoList, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = todoList.Id.MakeTodoListUri(url),
                Title = todoList.Name
            };
        }


        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource.
        ///     This is NOT a cacheable version because tenants are used BY VALUE
        /// </summary>
        /// <seealso cref="TodoCreateDataRepresentation" />
        public static CreateFormRepresentation ToTodoListCreateFormRepresentation(
            this IUrlHelper url,
            IEnumerable<Tenant> tenants)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // self this collection
                    url.MakeTodoListCreateFormUri().MakeWebLink(IanaLinkRelation.Self)

                    // no submit to make it cacheable
                },
                Items = MakeCreateFormItems(tenants, url)
            };
        }

        /// <summary>
        ///     Translate incoming todo list (POST data) into create data for creation in the store
        /// </summary>
        public static TodoListCreateData FromRepresentation(
            this TodoListCreateDataRepresentation todoList,
            string tenantId)
        {
            return new TodoListCreateData
            {
                Name = todoList.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo list requires a name"),

                Tenant = tenantId
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo list requires a tenant"),
            };
        }

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <remarks>
        ///     The edit form has no <see cref="IanaLinkRelation.Up" /> link to the
        ///     main resource, thus allowing the edit form to be the same for all instances of
        ///     the resource and thus fully cacheable.
        /// </remarks>
        public static EditFormRepresentation ToTodoListEditFormRepresentation(this IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    url.MakeTodoEditFormUri().MakeWebLink(IanaLinkRelation.Self)
                },
                Items = new FormItemRepresentation[]
                {
                    new TextInputFormItemRepresentation
                    {
                        Name = "name",
                        Description = "The title of the todo list",
                        Required = true
                    }
                }
            };
        }


        private static FormItemRepresentation[] MakeCreateFormItems(IEnumerable<Tenant> tenants, IUrlHelper url)
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The title of the todo list",
                    Required = true
                },
                new SelectFormItemRepresentation
                {
                    Name = "tenant",
                    Description = "A todo list must exist to one tenant",
                    Required = true,
                    Multiple = false,
                    Items = new List<SelectOptionItemRepresentation>(tenants.Select(tenant =>
                            new SelectOptionValueItemRepresentation
                            {
                                Value = tenant.Id.MakeTenantUri(url),
                                Type = FormType.Enum,
                                Description = tenant.Description,
                                Label = tenant.Code,
                                Name = tenant.Name
                            }))
                        .ToArray()
                }
            };
        }
    }
}