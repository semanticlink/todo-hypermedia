using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface IUserStore
    {
        Task<string> Create(string tenantId, string identityId);
        Task<User> Get(string id);
        Task<User> GetByIdentityId(string id);
        Task<User> GetByTenant(string tenantId);
        Task<IEnumerable<User>> GetAll();
        Task Update(string userId, Action<User> updater);
        Task Delete(string id);
    }
}