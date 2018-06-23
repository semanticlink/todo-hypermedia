using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Web
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        ///     Retrieves the user details from the <see cref="ControllerBase.User"/> from the JWT.
        /// </summary>
        public static User ToUser(this ClaimsPrincipal user)
        {
            return new User
            {
                Id = user.Value(JwtRegisteredClaimNames.Sub),
                Name = user.Value(JwtRegisteredClaimNames.Email),
                IdentityId = user.Value(JwtRegisteredClaimNames.Jti),
            };
        }

        private static string Value(this ClaimsPrincipal user, string type)
        {
            try
            {
                return user.FindFirst(type).Value;
            }
            catch (NullReferenceException e)
            {
                /**
                 *  An error here is likely to be a programming error
                 *  because either the method/class has no [Authorize] attribute
                 *  or incorrectly has [AllowAnonymous]
                 *
                 *  With either of these methods, no User.Principal is loaded onto the Context.User
                 *
                 *  see https://github.com/aspnet/Security/issues/1310
                 */
                throw new NullReferenceException(
                    $"Ensure method/class atrribute has correct [Authorize] added or incorrect [AllowAnonymous] removed or type '{type}' is not available as a claim");
            }
        }
    }
}