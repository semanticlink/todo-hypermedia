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
        [Obsolete]
        Task<string> Create(string identityId, UserCreateDataRepresentation data);

        /// <summary>
        ///     Create a new user from a trusted code. This method should only be used a seeding time to create a user.
        /// </summary>
        /// <param name="creator">User that is creating a user. This should be the root user</param>
        /// <param name="identityId">Id provided from third-party (or indeed identifier from internal) eg 'auth0|xxxxx' or 'service|xxxx'</param>
        /// <param name="data">The new user to be created</param>
        [Obsolete]
        Task<string> Create(User creator, string identityId, UserCreateDataRepresentation data);

        /// <summary>
        ///     This method should only be external called when using the a trusted user through trusted code
        ///     because it overrides the injected user from the context.
        /// </summary>
        /// <remarks>
        ///    KLUDGE: this is easier than trying to reset the constructor inject of <see cref="User"/>
        /// </remarks>
        /// <param name="creatorId">User id that is creating a user. This should be the root user</param>
        /// <param name="resourceId">Id of the resource to have user rights against</param>
        /// <param name="userExternalId">Id provided from third-party (or indeed identifier from internal) eg 'auth0|xxxxx' or 'service|xxxx'</param>
        /// <param name="data">The new user to be created</param>
        /// <param name="callerRights"></param>
        /// <param name="callerCollectionRights"></param>
        /// <returns></returns>
        Task<string> Create(
            string creatorId,
            string resourceId,
            string userExternalId,
            UserCreateDataRepresentation data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);

        Task<User> Get(string id);
        Task<User> GetByExternalId(string externalId);
        Task<bool> IsRegistered(string id);
        Task<IEnumerable<User>> GetAll();
        Task Update(string id, Action<User> updater);
        Task Delete(string id);
    }
}