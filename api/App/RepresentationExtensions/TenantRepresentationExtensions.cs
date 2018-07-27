using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.Forms;
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

        public static FeedRepresentation ToRepresentation(
            this IEnumerable<Tenant> tenants,
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

                    // edit form
                    url.MakeTenantCreateFormUri().MakeWebLink(IanaLinkRelation.CreateForm),

                    // make a search for a tenant
                    url.MakeHomeTenantsSearchFormUri().MakeWebLink(IanaLinkRelation.Search)
                },
                Items = tenants
                    .Select(c => ToFeedRepresentationItem(c, url))
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
                    url.MakeTenantCreateFormUri().MakeWebLink(IanaLinkRelation.Self)
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
    }
}