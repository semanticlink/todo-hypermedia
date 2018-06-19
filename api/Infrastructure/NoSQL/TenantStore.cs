using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TenantStore : ITenantStore
    {
        private readonly IDynamoDBContext _context;

        public TenantStore(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<string> Create(TenantCreateData tenant)
        {
            tenant.Code.ThrowInvalidDataExceptionIfNullOrWhiteSpace("Code cannot be empty");

            var id = Guid.NewGuid().ToString();

            var now = DateTime.UtcNow;
            var create = new Tenant
            {
                Id = id,
                Name = tenant.Name,
                Code = tenant.Code,
                Description = tenant.Description,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(create);

            return id;
        }

        public async Task<Tenant> Get(string id)
        {
            return await _context.FirstOrDefault<Tenant>(id);
        }

        public async Task<Tenant> GetByCode(string code)
        {
            return await _context.FirstOrDefault<Tenant>(nameof(Tenant.Code), code);
        }

        public async Task<IEnumerable<Tenant>> GetTenantsForUser(string id)
        {
            return await _context.Where<Tenant>(id);
        }

        public Task<IEnumerable<User>> GetUsersByTenant(string id)
        {
            throw new NotImplementedException();
        }
    }
}