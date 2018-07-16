using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Http;
using Toolkit;

namespace App
{
    /// <summary>
    ///     A wrapper to return the internal system identity of the authenticated user
    /// </summary>
    /// <remarks>
    ///     This code reproduces some of the <see cref="UserStore.GetByExternalId"/> to avoid circular dependencies
    ///     when the user is injected into it.
    /// </remarks>
    public class UserResolverService
    {
        private readonly IHttpContextAccessor _context;
        private readonly IDynamoDBContext _dbContext;

        public UserResolverService(IHttpContextAccessor context, IDynamoDBContext dbContext)
        {
            _context = context;
            _dbContext = dbContext;
        }

        /// <summary>
        ///     Looks up a user by their external Ids. 
        /// </summary>
        /// <remarks>
        ///    This code is assumed to be running only only a trusted code/account
        /// </remarks>
        private async Task<User> GetByExternalId(string externalId)
        {
            externalId
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("External Id must be set");

            return await _dbContext.FirstOrDefault<User>(
                new ScanCondition(nameof(User.ExternalIds), ScanOperator.Contains, externalId));
        }

        /// <summary>
        ///    This looks inside the current <see cref="HttpContext.User"/> that is resolved as a <see cref="ClaimsPrincipal"/>
        ///    and grabs the external <see cref="Claim"/> Id and resolves that to the internal <see cref="User"/>.
        /// </summary>
        public User GetUser()
        {
            return _context.HttpContext != null
                ? GetByExternalId(
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