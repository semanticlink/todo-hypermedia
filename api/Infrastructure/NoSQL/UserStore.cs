using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserStore : IUserStore
    {
        public const string TableName = TableNameConstants.User;
        private const string HashKey = HashKeyConstants.DEFAULT;
        private readonly IDynamoDBContext _context;

        public UserStore(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<string> Create(string tenantId, string identityId, string name)
        {
            var id = Guid.NewGuid().ToString();

            var now = DateTime.UtcNow;

            var create = new User
            {
                Id = id,
                IdentityId = identityId,
                TenantId = tenantId,
                Name = name,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(create);

            return id;
        }

        public async Task<User> Get(string id)
        {
            return await _context.SingleOrDefault<User>(id);
        }

        public async Task<User> GetByIdentityId(string identityId)
        {
            return await _context.SingleOrDefault<User>(nameof(User.IdentityId), identityId);
        }


        public async Task<User> GetByTenant(string tenantId)
        {
            return await _context.SingleOrDefault<User>(nameof(User.TenantId), tenantId);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Where<User>();
        }

        public async Task Update(string userId, Action<User> updater)
        {
            var user = await Get(userId)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(user);
            user.Id = userId;

            await _context.SaveAsync(user);
        }

        public async Task Delete(string id)
        {
            var user = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(user);
        }
    }
}