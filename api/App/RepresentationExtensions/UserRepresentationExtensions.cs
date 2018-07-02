using System.Collections.Generic;
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
    /// <summary>
    ///     A class to be able to read the Auth0 Id format and take in the the domain and spit out something
    ///     semi sensible
    /// <example>
    ///    auth0|5b32b696a8c12d3b9a32b138
    ///    rel = auth0
    ///    uri = https://somesubdomain.auth0.com/5b32b696a8c12d3b9a32b138
    ///    title = externalId
    /// </example> 
    /// </summary>
    /// 
    public class Auth0Id
    {
        private const char Delimiter = '|';
        private const string Rel = CustomLinkRelation.Authenticator;

        private string Id { get; set; }
        private string Title { get; set; }

        /// <summary>
        ///     Very simple parser on bar delimited
        /// </summary>
        /// <param name="id"></param>
        private void Parse(string id)
        {
            var provider = id.Split(Delimiter);

            if (provider.Length == 2)
            {
                Id = provider.Last();
                Title = provider.First();
            }
            else
            {
                // TODO logging
                // we don't have a format we recognise
            }
        }

        public WebLink MakeWebLink(string id, IUrlHelper url)
        {
            Parse(id);
            return Id.MakeUserAuthenticator(Title, url).MakeWebLink(Rel, Title);
        }

        public WebLink[] MakeWebLinks(List<string> ids, IUrlHelper url)
        {
            return ids
                .Select(id => MakeWebLink(id, url))
                .ToArray();
        }
    }


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

                        // todos
                        user.Id.MakeUserTodoCollectionUri(url).MakeWebLink(CustomLinkRelation.Todos),

                        // edit-form
                        url.MakeUserEditFormUri().MakeWebLink(IanaLinkRelation.EditForm)
                    }
                    .Concat(new Auth0Id().MakeWebLinks(user.ExternalIds, url))
                    .ToArray()
                    .RemoveNulls(),
                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
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