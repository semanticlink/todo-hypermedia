using System.Collections.Generic;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITenantRepository
    {
        Tenant Get(string id);
        Tenant GetByCode(string code);
        IEnumerable<Tenant> GetTenantsForUser(string userId);
    }
}