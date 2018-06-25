using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Persistence
{
    public interface ITenantStore
    {
        Task<string> Create(TenantCreateData tenant);
        Task<Tenant> Get(string id);
        Task<Tenant> GetByCode(string code);
        Task<IEnumerable<Tenant>> GetTenantsForUser(string userId);
        Task<IEnumerable<IdentityUser>> GetUsersByTenant(string id);
        /// <summary>
        ///     Add a user to a tenant, if the tenant exists.
        /// </summary>
        /// <param name="id">Tenant to get the new user</param>
        /// <param name="userId">User to be added</param>
        /// <returns></returns>
        Task AddUser(string id, string userId);
       /// <summary>
        ///     Remove a user from the tenant, if exists.
        /// </summary>
        /// <param name="id">Tenant to remove the existing user</param>
        /// <param name="userId">User to be removed</param>
        /// <returns></returns>
        Task RemoveUser(string id, string userId);
    }
}