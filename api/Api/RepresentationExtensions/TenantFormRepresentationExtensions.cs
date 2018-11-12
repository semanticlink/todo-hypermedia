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
    /// <seealso cref="TenantRepresentation" />
    /// <seealso cref="TenantRepresentationExtensions" />
    public static class TenantFormRepresentationExtensions
    {
        /// <summary>
        ///     A form to describe <see cref="TeantSearchRepresentation" />
        /// </summary>
        /// <seealso cref="TeantSearchRepresentation" />
        public static SearchFormRepresentation ToTenantSearchFormRepresentation(
            this TenantRepresentation nullTenant,
            IUrlHelper url)
        {
            return new SearchFormRepresentation
            {
                Links = new[]
                {
                    url.MakeHomeTenantsSearchFormUri().MakeWebLink(IanaLinkRelation.Self),
                    url.MakeHomeTenantsUri().MakeWebLink(IanaLinkRelation.Up),
                    url.MakeHomeTenantSearchUri().MakeWebLink(CustomLinkRelation.Submit),
                },
                Items = MakeSearchFormItems(),
            };
        }

        private static FormItemRepresentation[] MakeSearchFormItems()
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "search",
                    Description = "The exact name of the tenant (wildcards are not supported)"
                },
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