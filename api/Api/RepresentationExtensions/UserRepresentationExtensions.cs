using System.Linq;
using Api.UriFactory;
using Api.Web;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using Toolkit;

namespace Api.RepresentationExtensions
{
    public static class UserRepresentationExtensions
    {
        /// <summary>
        ///     A user representation
        /// </summary>
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


        /// <summary>
        ///     Reverse map with validation across-the-wire representation into in-memory representation
        /// </summary>
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
    }
}