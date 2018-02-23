using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.Db
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

        public Tenant Get(string id)
        {
            return _context.Tenants
                .SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<Tenant> GetTenantsForUser(string userId)
        {
            // TODO: filter tenants by user
            // TODO: make relationships between tables
            return _context.Tenants
                .Where(t => !t.Name.IsNullOrWhitespace());
        }
    }
}