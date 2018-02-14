using System;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;
using TodoApi.Utils;

namespace TodoApi.Db
{
    public class TenantRepository : ITenantRepository
    {
        private readonly TodoContext _context;

        public TenantRepository(TodoContext context)
        {
            _context = context;
        }

        public Tenant GetByCode(string code)
        {
            return _context.Tenants
                .SingleOrDefault(x => x.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));
        }

        public Tenant Get(long id)
        {
            return _context.Tenants
                .SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<Tenant> GetTenantsForUser(long userId)
        {
            // TODO: filter tenants by user
            // TODO: make relationships between tables
            return _context.Tenants
                .Where(t => !t.Name.IsNullOrWhitespace());
        }
    }
}