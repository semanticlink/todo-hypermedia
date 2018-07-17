using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NLog;
using Toolkit;

namespace App
{
    public static class AuthenticationExtensions
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieves the user Id from the <see cref="ControllerBase.User"/> from the JWT (<see cref="JwtRegisteredClaimNames.Sub"/>).
        /// </summary>
        public static string GetExternalId(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Sub);
        }

        /// <summary>
        /// Retrieves the user name (email) from the <see cref="ControllerBase.User"/> from the JWT (<see cref="JwtRegisteredClaimNames.Email"/>).
        /// </summary>
        public static string GetName(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Email);
        }

        /// <summary>
        /// Retrieves the user Identity Id from the <see cref="ControllerBase.User"/> from the JWT (<see cref="JwtRegisteredClaimNames.Jti"/>).
        /// </summary>
        public static string GetIdentityId(this ClaimsPrincipal user)
        {
            return user.Value(JwtRegisteredClaimNames.Jti);
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