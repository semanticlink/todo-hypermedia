using Microsoft.AspNetCore.Mvc;
using TodoApi.LinkRelations;
using TodoApi.Representation;
using TodoApi.Representation.Forms;
using TodoApi.Representation.LinkedRepresentation;
using TodoApi.UriFactory;

namespace TodoApi.RepresentationExtensions
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
    }
}