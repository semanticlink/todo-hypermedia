using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TenantStore : ITenantStore
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly IUserRightStore _userRightStore;

        public TenantStore(IDynamoDBContext context, IIdGenerator idGenerator, IUserRightStore userRightStore)
        {
            _context = context;
            _idGenerator = idGenerator;
            _userRightStore = userRightStore;
        }

        public async Task<string> Create(string creatorId, TenantCreateData data)
        {
            data.Code.ThrowInvalidDataExceptionIfNullOrWhiteSpace("Code cannot be empty");

            var now = DateTime.UtcNow;

            var tenant = new Tenant
            {
                Id = _idGenerator.New(),
                Name = data.Name,
                Code = data.Code,
                Description = data.Description,
                CreatedBy = creatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(tenant);

            Log.TraceFormat("New tenant {0} created by user {1}", tenant.Id, creatorId);

            return tenant.Id;
        }

        public async Task<string> Create(
            string creatorId,
            string resourceId,
            string userExternalId,
            TenantCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            var tenantId = await Create(creatorId, data);

            await _userRightStore.CreateRights(
                tenantId,
                tenantId,
                RightType.User.MakeCreateRights(callerRights, callerCollectionRights),
                new InheritForm
                {
                    Type = RightType.RootUserCollection,
                    ResourceId = resourceId,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Tenant,
                        RightType.TenantTodoCollection,
                        RightType.TenantUserCollection
                    }
                });
            
            return tenantId;
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
            var usersByTenant = (await Get(id)).User;
            return !usersByTenant.IsNullOrEmpty()
                ? usersByTenant
                : new List<string>();
        }

        public async Task IncludeUser(string id, string userId)
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

        public async Task IncludeUser(string id, string userId, Permission callerRights)
        {
            await IncludeUser(id, userId);

            await _userRightStore.SetRight(userId, id, RightType.Tenant, callerRights);
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

            await _userRightStore.RemoveRight(userId, id, RightType.Tenant);
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