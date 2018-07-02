using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface IUserStore
    {
        Task<string> Create(string identityId, string email, string name);
        Task<User> Get(string id);
        Task<User> GetByExternalId(string externalId);
        Task<bool> IsRegistered(string externalId);
        Task<IEnumerable<User>> GetAll();
        Task Update(string id, Action<User> updater);
        Task Delete(string id);
    }
}