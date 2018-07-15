using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TenantStore : ITenantStore
    {
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;

        public TenantStore(IDynamoDBContext context, IIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<string> Create(TenantCreateData tenant)
        {
            tenant.Code.ThrowInvalidDataExceptionIfNullOrWhiteSpace("Code cannot be empty");

            var now = DateTime.UtcNow;
            
            var create = new Tenant
            {
                Id = _idGenerator.New(),
                Name = tenant.Name,
                Code = tenant.Code,
                Description = tenant.Description,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(create);

            return create.Id;
        }

        public async Task<Tenant> Get(string id)
        {
            return await _context.FirstOrDefault<Tenant>(id);
        }

        public async Task<Tenant> GetByCode(string code)
        {
            return await _context.FirstOrDefault<Tenant>(nameof(Tenant.Code), code);
        }

        public async Task<IEnumerable<Tenant>> GetTenantsForUser(string userId)
        {
            return await _context.Where<Tenant>(new ScanCondition(nameof(Tenant.User), ScanOperator.Contains, userId));
        }

        public async Task<IEnumerable<string>> GetUsersByTenant(string id)
        {
            var usersByTenant = (await  Get(id)).User;
            return !usersByTenant.IsNullOrEmpty()
                ? usersByTenant
                : new List<string>();
        }

        public async Task AddUser(string id, string userId)
        {
            await Update(id, tenant =>
            {
                var newId = new List<string> {userId};

                tenant.User = tenant.User.IsNullOrEmpty()
                    ? newId
                    : tenant.User
                        .Concat(newId)
                        // sure no duplicates
                        .Distinct()
                        .ToList();
            });
        }

        public async Task RemoveUser(string id, string userId)
        {
            await Update(id, tenant =>
            {
                if (tenant.User.IsNotNull())
                {
                    tenant.User.Remove(userId);
                }
            });
        }

        private async Task Update(string id, Action<Tenant> updater)
        {
            var tenant = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            // TODO: make Id, Created immutable

            updater(tenant);

            // if tags have been removed, it looks like you can't hand
            // though an empty list but rather need to null it.
            // TODO: check this is true
            tenant.User = !tenant.User.IsNullOrEmpty() ? tenant.User : null;

            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(tenant);
        }
    }
}