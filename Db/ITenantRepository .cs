using System.Collections.Generic;
using TodoApi.Models;

namespace TodoApi.Db
{
    public interface ITenantRepository
    {
        Tenant Get(long id);
        Tenant GetByCode(string code);
        IEnumerable<Tenant> GetTenantsForUser(long userId);
    }
}