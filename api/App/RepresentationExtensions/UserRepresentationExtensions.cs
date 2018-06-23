using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.LinkRelations;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class UserRepresentationExtensions
    {
        public static UserRepresentation ToRepresentation(this IdentityUser user, string tenantId, IUrlHelper url)
        {
            return new UserRepresentation
            {
                Links = new[]
                {
                    // self
                    user.Id.MakeUserUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // logical parent of user is a tenant
                    tenantId.MakeTenantUri(url).MakeWebLink(IanaLinkRelation.Up),

                    user.Id.MakeUserTodoCollectionUri(url).MakeWebLink(CustomLinkRelation.Todos),

                    // edit-form
                    url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.EditForm)
                },
            };
        }

        public static FeedRepresentation ToFeedRepresentation(this IEnumerable<User> users, IUrlHelper url)
        {
            return new FeedRepresentation
            {
                Links = new[]
                {
                    // self
                    url.MakeUserCollectionUri().MakeWebLink(IanaLinkRelation.Self),

                    // up link to root
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Up),

                    // todos
                    url.MakeTodoCollectionUri().MakeWebLink(CustomLinkRelation.Todos),
                },
                Items = users
                    .Select(t => t.MakeUserFeedItemRepresentation(url))
                    .ToArray()
            };
        }

        private static FeedItemRepresentation MakeUserFeedItemRepresentation(this User user, IUrlHelper url)
        {
            return new FeedItemRepresentation
            {
                Id = user.IdentityId.MakeUserUri(url),
                Title = user.Name
            };
        }


        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource
        /// </summary>
        /// <seealso cref = "UserCreateDataRepresentation" />
        public static CreateFormRepresentation ToUserCreateFormRepresentation(this IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    url.MakeUserCreateFormUri().MakeWebLink(IanaLinkRelation.Self),

                    // Create a new organisation on the collection
//                    tenantId.MakeTenantUsersCollectionUri(url).MakeWebLink(IanaLinkRelation.Up),
                },
                Items = MakeCreateFormItems()
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
                    Description = "The name of the user to be shown on the screen"
                },
            };
        }

        public static UserCreateData FromRepresentation(this UserCreateDataRepresentation user, IUrlHelper url)
        {
            return new UserCreateData
            {
                Email = user.Email
                    .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A user requires an email"),
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
                }
            };
        }
    }
}