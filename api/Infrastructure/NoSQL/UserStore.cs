using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserStore : IUserStore
    {
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;

        public UserStore(IDynamoDBContext context, IIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<string> Create(string externalId, string email, string name)
        {
            (await GetByExternalId(externalId))
                .ThrowInvalidDataExceptionIfNotNull("User already created");

            // KLUDGE: both need to be injected
            var now = DateTime.UtcNow;

            var create = new User
            {
                Id = _idGenerator.New(),
                ExternalIds = new List<string> {externalId},
                Email = email,
                Name = name,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(create);

            return create.Id;
        }

        public async Task<User> Get(string id)
        {
            return await _context.SingleOrDefault<User>(id);
        }

        public async Task<User> GetByExternalId(string externalId)
        {
            externalId
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("External Id must be set");

            return await _context.FirstOrDefault<User>(
                new ScanCondition(nameof(User.ExternalIds), ScanOperator.Contains, externalId));
        }

        public async Task<bool> IsRegistered(string externalId)
        {
            return (await GetByExternalId(externalId)).IsNotNull();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Where<User>();
        }

        public async Task Update(string id, Action<User> updater)
        {
            var user = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(user);
            user.Id = id;

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