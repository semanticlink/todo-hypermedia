using System.Collections.Generic;
using System.Linq;
using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using SemanticLink.Form;
using Toolkit;

namespace Api.RepresentationExtensions
{
    public static class TenantRepresentationExtensions
    {
        public static TenantRepresentation ToRepresentation(this Tenant tenant, IUrlHelper url)
        {
            return new TenantRepresentation
            {
                Links = new[]
                {
                    // self
                    tenant.Id.MakeTenantUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of tenant is root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // users over the entire tenant
                    tenant.Id.MakeTenantUsersUri(url).MakeWebLink(CustomLinkRelation.Users),

                    // edit form
                    url.MakeTenantEditFormUri().MakeWebLink(IanaLinkRelation.EditForm),
                },


                Code = tenant.Code,
                Name = tenant.Name,
                Description = tenant.Description
            };
        }

        /// <summary>
        ///    Tenant for a user with access to the user's todos (as well as the tenants users that that user has access to)     
        /// </summary>
        public static TenantRepresentation ToRepresentation(this Tenant tenant, string userId, IUrlHelper url)
        {
            return new TenantRepresentation
            {
                Links = new[]
                {
                    // self
                    userId.MakeUserTenantUri(tenant.Id, url).MakeWebLink(IanaLinkRelation.Self),

                    // canonical
                    tenant.Id.MakeTenantUri(url).MakeWebLink(IanaLinkRelation.Canonical),

                    // logical parent of user tenant is user
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // tenant todos
                    userId.MakeUserTenantTodoListUri(tenant.Id, url).MakeWebLink(CustomLinkRelation.Todos),

                    // users over the entire tenant
                    tenant.Id.MakeTenantUsersUri(url).MakeWebLink(CustomLinkRelation.Users),

                    // edit form
                    url.MakeTenantEditFormUri().MakeWebLink(IanaLinkRelation.EditForm),
                },

                Code = tenant.Code,
                Name = tenant.Name,
                Description = tenant.Description
            };
        }


        public static FeedRepresentation ToSearchFeedRepresentation(this IEnumerable<Tenant> tenants,
            string userId,
            string criteria,
            IUrlHelper url)
        {
            var feedRepresentation = new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    criteria.MakeHomeTenantsUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // home is the logical parent
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // create form
/*
                    url.MakeTenantCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm),
*/

                    // make a search for a tenant
                    url.MakeHomeTenantsSearchFormUri().MakeWebLink(IanaLinkRelation.Search)
                },
                Items = tenants
                    .Select(c => ToSearchFeedRepresentationItem(c, userId, url))
                    .ToArray()
            };
            return feedRepresentation;
        }

        public static FeedRepresentation ToRepresentation(
            this IEnumerable<string> userIds,
            string tenantId,
            IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    tenantId.MakeTenantUsersUri(url).MakeWebLink(IanaLinkRelation.Self),

                    tenantId.MakeTenantUri(url).MakeWebLink(IanaLinkRelation.Up),

                    url.MakeUserCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = userIds
                    .Select(id => ToFeedRepresentationItem(id, url))
                    .ToArray()
            };
        }


        public static FeedRepresentation ToTenantFeedRepresentation(
            this IEnumerable<Tenant> tenants,
            string userId,
            IUrlHelper url)
        {
            var feedRepresentation = new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    userId.MakeUserTenantsUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // home is the logical parent
                    userId.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // create-form
                    url.MakeTenantCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = tenants
                    .Select(tenant => tenant.ToUserTenantFeedRepresentationItem(userId, url))
                    .ToArray()
            };
            return feedRepresentation;
        }


        private static FeedItemRepresentation ToUserTenantFeedRepresentationItem(
            this Tenant tenant,
            string userId,
            IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = userId.MakeUserTenantUri(tenant.Id, url),
                Title = tenant.Name
            };
        }

        private static FeedItemRepresentation ToSearchFeedRepresentationItem(
            this Tenant tenant,
            string userId,
            IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = userId.MakeUserTenantUri(tenant.Id, url),
                Title = tenant.Name
            };
        }

        private static FeedItemRepresentation ToFeedRepresentationItem(this string userId, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = userId.MakeUserUri(url)
            };
        }

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref="UserCreateDataRepresentation" />
        public static CreateFormRepresentation ToTenantCreateFormRepresentation(this IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    url.MakeTenantCreateFormUri().MakeWebLink(IanaLinkRelation.Self),
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
                    Name = "code",
                    Description = "The unique name in domain name format",
                    Required = true
                },
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The name of the tenant to be shown on the screen",
                    Required = false
                },
                new TextInputFormItemRepresentation
                {
                    Name = "description",
                    Description = "Other details about the organisation",
                    Required = false
                },
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
        public static EditFormRepresentation ToTenantEditFormRepresentation(this IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    url.MakeTenantEditFormUri().MakeWebLink(IanaLinkRelation.Self)
                },
                Items = MakeFormItems()
            };
        }


        public static TenantCreateData FromRepresentation(this TenantCreateDataRepresentation tenant)
        {
            return new TenantCreateData
            {
                Name = tenant.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A tenant requires a name"),

                // TODO: perhaps the code needs validation as per a subdomain for SaaS business
                Code = tenant.Code,
                Description = tenant.Description
            };
        }
    }
}