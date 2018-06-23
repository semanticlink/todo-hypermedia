using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface IUserStore
    {
        Task<string> Create(string tenantId, string identityId, string name);
        Task<User> Get(string id);
        Task<User> GetByName(string nameAsEmail);
        Task<User> GetByTenant(string tenantId);
        Task<IEnumerable<User>> GetAll();
        Task Update(string userId, Action<User> updater);
        Task Delete(string id);
    }
}