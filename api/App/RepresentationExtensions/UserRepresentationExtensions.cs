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
    public static class UserRepresentationExtensions
    {
        public static UserRepresentation ToRepresentation(this User user,
            IUrlHelper url)
        {
            return new UserRepresentation
            {
                Links = new[]
                    {
                        // self
                        user.Id.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Self),

                        // logical parent of user is home
                        url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                        // named todo lists
                        user.Id.MakeUserTodosUri(url).MakeWebLink(CustomLinkRelation.Todos),

                        // user tenants
                        user.Id.MakeUserTenantsUri(url).MakeWebLink(CustomLinkRelation.Tenants),

                        // edit-form
                        url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.EditForm),
                    }
                    // authentication
                    .Concat(new Auth0Id().MakeWebLinks(user.ExternalIds, url))
                    .ToArray()
                    .RemoveNulls(),
                Email = user.Email,
                Name = user.Name,
                ExternalIds = user.ExternalIds
            };
        }


        public static UserCreateData FromRepresentation(this UserCreateDataRepresentation data)
        {
            return new UserCreateData
            {
                Name = data.Name
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A user requires a name"),
                Email = data.Email
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A user requires an email"),
                ExternalId = data.ExternalId
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A user requires an external id")
            };
        }

        /////////////////////////////
        //
        // Forms
        // =====
        //

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref="UserCreateDataRepresentation" />
        public static CreateFormRepresentation ToRegisterUserCreateFormRepresentation(
            this string tenantId,
            IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    tenantId.MakeRegisterUserCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),
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