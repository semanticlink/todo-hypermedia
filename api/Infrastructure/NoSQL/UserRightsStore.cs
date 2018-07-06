using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserRightsStore
    {
        private readonly IDynamoDBContext _dbContext;

        public UserRightsStore(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Create(
            string userId,
            string resourceId,
            ResourceType resourceType,
            Permissions permissions)
        {
            var userRights = (await Get(userId, resourceId));

            if (userRights.IsNotNull())
            {
                return userRights.Id;
            }

            var id = IdGenerator.New();

            userRights = new UserRights
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permissions,
                Type = resourceType
            };

            await _dbContext.SaveAsync(userRights);

            return id;
        }

        public async void Update(string userId, string resourceId, Permissions permissions)
        {
            var userRight = (await Get(userId, resourceId))
                .ThrowObjectNotFoundExceptionIfNull($"Users rights not found: '{userId}' '{resourceId}'");

            userRight.Rights = permissions;

            await _dbContext.SaveAsync(userRight);
        }


        public async Task<UserRights> Get(string userId, string resourceId)
        {
            return await _dbContext.SingleOrDefault<UserRights>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRights.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRights.ResourceId), ScanOperator.Equal, resourceId)
            });
        }
    }
}