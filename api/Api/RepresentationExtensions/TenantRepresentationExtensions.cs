using System.Collections.Generic;
using System.Linq;
using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;

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

    }
}