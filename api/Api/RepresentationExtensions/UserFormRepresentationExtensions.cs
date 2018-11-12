using Api.UriFactory;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using SemanticLink.Form;

namespace Api.RepresentationExtensions
{
    public static class UserFormRepresentationExtensions
    {
 
        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref="UserCreateDataRepresentation" />
        public static CreateFormRepresentation ToUserCreateFormRepresentation(this IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    url.MakeUserCreateFormUri().MakeWebLink(IanaLinkRelation.Self),
                },
                Items = MakeCreateFormItems()
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
        public static EditFormRepresentation ToUserEditFormRepresentation(this IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.Self)
                },
                Items = MakeFormItems()
            };
        }

        private static FormItemRepresentation[] MakeFormItems()
        {
            return new FormItemRepresentation[]
            {
                new EmailInputFormItemRepresentation
                {
                    Name = "email",
                    Description = "The email address of the user",
                    Required = true
                },
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The name of the user to be shown on the screen",
                    Required = true
                },
                new SelectFormItemRepresentation
                {
                    Name = "externalId",
                    Description = "The third-party id fo the user (eg 'auth0|xxxxx')",
                    Required = false,
                    Multiple = true
                }
            };
        }

        private static FormItemRepresentation[] MakeCreateFormItems()
        {
            return new FormItemRepresentation[]
            {
                new EmailInputFormItemRepresentation
                {
                    Name = "email",
                    Description = "The email address of the user",
                    Required = true
                },
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The name of the user to be shown on the screen",
                    Required = true
                },
                new TextInputFormItemRepresentation
                {
                    Name = "externalId",
                    Description = "The third-party id fo the user (eg 'auth0|xxxxx')",
                    Required = true
                }
            };
        }
    }
}