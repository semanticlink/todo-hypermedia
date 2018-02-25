using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITenantStore
    {
        Task<string> Create(TenantCreateData tenant);
        Task<Tenant> Get(string id);
        Task<Tenant> GetByCode(string code);
        Task<IEnumerable<Tenant>> GetTenantsForUser(string id);
    }
}