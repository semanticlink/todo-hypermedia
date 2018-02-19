using System.Collections.Generic;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITenantRepository
    {
        Tenant Get(long id);
        Tenant GetByCode(string code);
        IEnumerable<Tenant> GetTenantsForUser(long userId);
    }
}