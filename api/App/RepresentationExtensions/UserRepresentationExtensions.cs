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
    public static class UserRepresentationExtensions
    {
        public static UserRepresentation ToRepresentation(this User user, IUrlHelper url)
        {
            return new UserRepresentation
            {
                Links = new[]
                {
                    // self
                    user.Id.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of user is home
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // todos
                    user.Id.MakeUserTodoCollectionUri(url).MakeWebLink(CustomLinkRelation.Todos),

                    // edit-form
                    url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.EditForm)
                },

                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
                ExternalIds = user.ExternalIds
            };
        }

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref = "UserCreateDataRepresentation" />
        public static CreateFormRepresentation ToRegisterUserCreateFormRepresentation(this string tenantId,
            IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    tenantId.MakeRegisterUserCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // Create a new organisation on the collection
                    tenantId.MakeTenantUsersUri(url).MakeWebLink(IanaLinkRelation.Up),

                    // submit
                    tenantId.MakeTenantUsersUri(url).MakeWebLink(CustomLinkRelation.Submit)
                },
                Items = MakeRegisterUserCreateFormItems()
            };
        }

        private static FormItemRepresentation[] MakeRegisterUserCreateFormItems()
        {
            return new FormItemRepresentation[]
            {
                new EmailInputFormItemRepresentation
                {
                    Name = "email",
                    Description = "The email address of the user",
                    Required = false
                },
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "The name of the user to be shown on the screen",
                    Required = false
                },
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
        public static EditFormRepresentation ToUserEditFormRepresentation(this IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.Self),
                },
                Items = MakeEditFormItems()
            };
        }

        private static FormItemRepresentation[] MakeEditFormItems()
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
                    Description = "The name of the user to be shown on the screen"
                },
                new CollectionInputFormItemRepresentation()
                {
                    Name = "externalIds",
                    Description = "Linked systems of authentication [not implemented]",
                    Multiple = true,
                    Required = false,
                }
            };
        }
    }
}