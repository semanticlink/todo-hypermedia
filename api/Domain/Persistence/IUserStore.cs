using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Representation;

namespace Domain.Persistence
{
    public interface IUserStore
    {
        /// <summary>
        ///     Create users 
        /// </summary>
        /// <param name="identityId">Id provided from third-party (or indeed identifier from internal) eg 'auth0|xxxxx' or 'service|xxxx'</param>
        /// <param name="data">The new user to be created</param>
        Task<string> Create(string identityId, UserCreateDataRepresentation data);

        /// <summary>
        ///     Create a new user from a trusted code. This method should only be used a seeding time to create a user.
        /// </summary>
        /// <param name="user">User that is creating a user. This should be the root user</param>
        /// <param name="identityId">Id provided from third-party (or indeed identifier from internal) eg 'auth0|xxxxx' or 'service|xxxx'</param>
        /// <param name="data">The new user to be created</param>
        Task<string> CreateByUser(User user, string identityId, UserCreateDataRepresentation data);
        /// <summary>
        ///     This method should only be external called when using the a trusted user through trusted code
        ///     because it overrides the injected user from the context.
        /// </summary>
        /// <remarks>
        ///    KLUDGE: this is easier than trying to reset the constructor inject of <see cref="User"/>
        /// </remarks>
        Task<string> CreateByUser(
            User user,
            string homeCollectionId,
            string identityId,
            UserCreateDataRepresentation data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);
        Task<User> Get(string id);
        Task<User> GetByExternalId(string externalId);
        Task<bool> IsRegistered(string externalId);
        Task<IEnumerable<User>> GetAll();
        Task Update(string id, Action<User> updater);
        Task Delete(string id);
    }
}