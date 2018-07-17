using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Models;
using NLog;
using Toolkit;

namespace App
{
    public static class AuthenticationExtensions
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private const string UserIdClaimKey = "userId";

        /// <summary>
        ///     Add a <see cref="User.Id"/> onto the claim as a <see cref="JwtRegisteredClaimNames.Jti"/>
        /// </summary>
        /// <seealso cref="GetIdentityId"/>
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
        /// Retrieves the user Id from the <see cref="User"/> from the JWT (<see cref="JwtRegisteredClaimNames.Sub"/>).
        /// </summary>
        public static string GetExternalId(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Sub);
        }

        /// <summary>
        /// Retrieves the user name (email) from the <see cref="User"/> from the JWT (<see cref="JwtRegisteredClaimNames.Email"/>).
        /// </summary>
        public static string GetName(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Email);
        }

        /// <summary>
        /// Retrieves the user Identity Id from the <see cref="User"/> from the claim via <see cref="UserIdClaimKey"/>.
        /// </summary>
        public static string GetIdentityId(this ClaimsPrincipal user)
        {
            return user.Value(UserIdClaimKey);
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
                 *
                 */
                Log.DebugExceptionFormat(
                    e,
                    $"Ensure method/class atrribute has correct [Authorize] added or incorrect [AllowAnonymous] removed or type '{type}' is not available as a claim"
                );
                return string.Empty;
            }
        }
    }
}