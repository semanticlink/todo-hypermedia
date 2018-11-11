using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Api.Web
{
    public static class AuthenticationExtensions
    {
        private const string UserIdClaimKey = "userId";

        /// <summary>
        ///     Add a <see cref="User.Id" /> onto the claim as a <see cref="JwtRegisteredClaimNames.Jti" />
        /// </summary>
        /// <seealso cref="GetId" />
        public static void AddIdentityIdToClaims(this ClaimsPrincipal user, string userId)
        {
            // Add the user id to the claim 
            var claims = new List<Claim>
            {
                new Claim(UserIdClaimKey, userId)
            };
            var appIdentity = new ClaimsIdentity(claims);

            user.AddIdentity(appIdentity);
        }

        /// <summary>
        ///     Retrieves the user Id from the <see cref="User" /> from the JWT (<see cref="JwtRegisteredClaimNames.Sub" />).
        /// </summary>
        public static string GetExternalId(this ClaimsPrincipal user, ILogger log = null)
        {
            return user.Value(JwtRegisteredClaimNames.Sub, log);
        }

        /// <summary>
        ///     Retrieves the user name (email) from the <see cref="User" /> from the JWT (
        ///     <see cref="JwtRegisteredClaimNames.Email" />).
        /// </summary>
        public static string GetName(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Email);
        }

        /// <summary>
        ///     Retrieves the user Identity Id from the <see cref="User" /> from the claim via <see cref="UserIdClaimKey" />.
        /// </summary>
        public static string GetId(this ClaimsPrincipal user, ILogger log = null)
        {
            return user.Value(UserIdClaimKey, log);
        }

        private static string Value(this ClaimsPrincipal user, string type, ILogger log = null)
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
                     *
                     */
                log?.DebugExceptionFormat(
                    e,
                    "No claim '{0}' found. Either no 'jwt' token sent or ensure method/class " +
                    "atrribute has correct [Authorize] added or incorrect [AllowAnonymous] removed",
                    type);
                return string.Empty;
            }
        }
    }
}