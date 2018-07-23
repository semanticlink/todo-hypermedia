using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITenantStore
    {
        [Obsolete]
        Task<string> Create(TenantCreateData data);

        Task<string> Create(string ownerid,
            string resourceId,
            TenantCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);

        Task<Tenant> Get(string id);
        Task<Tenant> GetByCode(string code);
        Task<IEnumerable<Tenant>> GetTenantsForUser(string userId);
        Task<IEnumerable<string>> GetUsersByTenant(string id);

        /// <summary>
        ///     Is the user (via their external identifier) already registered on this tenant
        /// </summary>
        /// <param name="id">Tenant id</param>
        /// <param name="userId">User id</param>
        Task<bool> IsRegisteredOnTenant(string id, string userId);

        /// <summary>
        ///     Add a user to a tenant, if the tenant exists.
        /// </summary>
        /// <param name="id">Tenant to get the new user</param>
        /// <param name="userId">User to be added</param>
        /// <returns></returns>
        [Obsolete]
        Task IncludeUser(string id, string userId);

        /// <summary>
        ///     Add a user to a tenant, if the tenant exists.
        /// </summary>
        /// <param name="id">Tenant to get the new user</param>
        /// <param name="userId">User to be added</param>
        /// <param name="callerRights"></param>
        /// <param name="callerCollectionRights"></param>
        /// <returns></returns>
        Task IncludeUser(
            string id,
            string userId,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);

        /// <summary>
        ///     Remove a user from the tenant, if exists.
        /// </summary>
        /// <param name="id">Tenant to remove the existing user</param>
        /// <param name="userId">User to be removed</param>
        /// <returns></returns>
        Task RemoveUser(string id, string userId);
    }
}