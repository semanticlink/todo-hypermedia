using System.Collections;
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
    /// <seealso cref="AuthenticateRepresentation" />
    /// <seealso cref="AuthenticateLoginRepresentation" />
    public static class AuthenticateRepresentationExtensions
    {
        /// <summary>
        ///     A form to describe <see cref="AuthenticateLoginRepresentation" />
        /// </summary>
        public static SearchFormRepresentation ToAuthenticateLoginFormRepresentation(
            this UserRepresentation nullUser,
            IUrlHelper url)
        {
            return new SearchFormRepresentation
            {
                Links = new[]
                {
                    url.MakeAuthenticateLoginFormUri().MakeWebLink(IanaLinkRelation.Self),
                    url.MakeHomeTenantsUri().MakeWebLink(IanaLinkRelation.Up),
                    url.MakeAuthenticateUri().MakeWebLink(CustomLinkRelation.Submit, "Login"),
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
                    Name = "email",
                    Description = "The email of the user.",
                    Required = true
                },
                new PasswordInputFormItemRepresentation
                {
                    Name = "password",
                    Required = true,
                    Description = "The password for the email credential."
                }
            };
        }
    }
}