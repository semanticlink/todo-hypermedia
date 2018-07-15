using System.Security.Claims;
using Domain.Models;
using Domain.Persistence;
using Microsoft.AspNetCore.Http;
using Toolkit;

namespace App
{
    /// <summary>
    ///     A wrapper to return the internal system identity of the authenticated user
    /// </summary>
    /// <remarks>
    ///    This looks inside the current <see cref="HttpContext.User"/> that is resolved as a <see cref="ClaimsPrincipal"/>
    ///    and grabs the external <see cref="Claim"/> Id and resolves that to the internal <see cref="User"/>.
    /// </remarks>
    public class UserResolverService
    {
        private readonly IHttpContextAccessor _context;
        private readonly IUserStore _userStore;

        public UserResolverService(IHttpContextAccessor context, IUserStore userStore)
        {
            _context = context;
            _userStore = userStore;
        }

        public User GetUser()
        {
            return _context.HttpContext != null
                ? _userStore.GetByExternalId(
                        _context.HttpContext
                            .User
                            .ThrowObjectNotFoundExceptionIfNull("User not found")
                            .GetExternalId()
                            .ThrowObjectNotFoundExceptionIfNull("User not found"))
                    .GetAwaiter()
                    .GetResult()
                    .ThrowObjectNotFoundExceptionIfNull("User not found")
                : new User();
        }
    }
}