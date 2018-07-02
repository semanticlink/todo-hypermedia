using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
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

                    // todos over the entire tenant
                    // url.MakeTodoCollectionUri().MakeWebLink(CustomLinkRelation.Todos)
                    
                    // users over the entire tenant
                    tenant.Id.MakeTenantUsersUri(url).MakeWebLink(CustomLinkRelation.Users)
                },

                Code = tenant.Code,
                Name = tenant.Name,
                Description = tenant.Description,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };
        }

        public static FeedRepresentation ToRepresentation(this IEnumerable<Tenant> tenants, string criteria, IUrlHelper url)
        {
            var feedRepresentation = new FeedRepresentation
            {
                Links = new[]
                {
                    criteria.MakeHomeTenantsUri(url).MakeWebLink(IanaLinkRelation.Self),
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),
                    url.MakeHomeTenantsSearchFormUri().MakeWebLink(IanaLinkRelation.Search),
                },
                Items = tenants
                    .Select(c => ToFeedRepresentationItem(c, url))
                    .ToArray()
            };
            return feedRepresentation;
        }

        public static FeedRepresentation ToRepresentation(this IEnumerable<string> userIds, string tenantId, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    tenantId.MakeTenantUsersUri(url).MakeWebLink(IanaLinkRelation.Self),
                    tenantId.MakeTenantUri(url).MakeWebLink(IanaLinkRelation.Up),
                    tenantId.MakeRegisterUserCreateFormUri(url).MakeWebLink(IanaLinkRelation.CreateForm)
                },
                Items = userIds
                    .Select(id => ToFeedRepresentationItem(id, url))
                    .ToArray()
            };
        }


        private static FeedItemRepresentation ToFeedRepresentationItem(this Tenant tenant, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = tenant.Id.MakeTenantUri(url),
                Title = tenant.Code
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