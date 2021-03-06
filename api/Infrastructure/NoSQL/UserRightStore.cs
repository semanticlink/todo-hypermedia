﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserRightStore : IUserRightStore
    {
        private ILogger Log { get; }
        private readonly IDynamoDBContext _dbContext;
        private readonly IIdGenerator _idGenerator;

        public UserRightStore(IDynamoDBContext dbContext, IIdGenerator idGenerator, ILogger<UserRightStore> log)
        {
            Log = log;
            _dbContext = dbContext;
            _idGenerator = idGenerator;
        }

        public async Task<string> SetRight(
            string userId,
            string resourceId,
            RightType type,
            Permission permission)
        {
            var userRight = await Get(userId, resourceId, type);

            if (userRight.IsNotNull()) return userRight.Id;

            var id = _idGenerator.New();

            userRight = new UserRight
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permission,
                Type = type
            };

            Log.TraceFormat(
                "Add right: user {0} to [{1:G},{2}] => rights {3:X} ({3:G})",
                userId,
                type,
                resourceId,
                permission);

            await _dbContext.SaveAsync(userRight);

            return id;
        }

        public async Task<string> SetInherit(
            RightType inheritType,
            string userId,
            string resourceId,
            RightType rightType,
            Permission permission)
        {
            var inheritRight = await GetInherit(userId, resourceId, rightType);

            if (inheritRight.IsNotNull()) return inheritRight.Id;

            var id = _idGenerator.New();

            inheritRight = new UserInheritRight
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permission,
                Type = rightType,
                InheritType = inheritType
            };
            
            Log.TraceFormat(
                "Create inherit right: user {0} to [{1:G},{2}] => rights {3:X} ({3:G}) inhert [{4:G}]",
                userId,
                rightType,
                resourceId,
                permission,
                inheritType);

            await _dbContext.SaveAsync(inheritRight);

            return id;
        }

        public async Task CreateRights(
            string userId,
            string resourceId,
            IDictionary<RightType, Permission> granted,
            InheritForm resource = null)
        {
            Log.DebugFormat("Creating rights on resource {0} for user {1}", resourceId, userId);

            // Add the new explicitly granted rights (for the user)
            foreach (var right in granted) await SetRight(userId, resourceId, right.Key, right.Value);

            // now deal with the inherit rights (for all users)
            if (resource != null)
            {
                foreach (var inheritType in resource.InheritedTypes)
                {
                    var inherit = await GetInheritRights(resource.Type, resource.ResourceId, inheritType);

                    foreach (var right in inherit)
                    {
                        Log.TraceFormat(
                            "INHERIT: from [{0},{1}], grant {2:X4} to {3} on [{4},{5}]",
                            resource.Type,
                            resource.ResourceId,
                            right.Rights,
                            right.UserId,
                            inheritType,
                            resourceId);

                        await SetRight(
                            right.UserId,
                            resourceId,
                            right.InheritType,
                            right.Rights | Permission.Inherited);
                    }
                }

                // CopyInheritTypes
                if (resource.CopyInheritTypes != null)
                    foreach (var copyInheritType in resource.CopyInheritTypes)
                    {
                        var inheritRights = await GetInheritRights(resource.Type, resource.ResourceId, copyInheritType);

                        foreach (var right in inheritRights)
                        {
                            Log.TraceFormat(
                                "COPY INHERIT: from [{0},{1}], grant {2:X4} to {3} on [{4}]",
                                resource.Type,
                                resource.ResourceId,
                                right.Rights,
                                right.UserId,
                                right.InheritType);

                            await SetInherit(
                                right.Type,
                                right.UserId,
                                resourceId,
                                right.InheritType,
                                right.Rights | Permission.None);
                        }
                    }
            }
        }

        public async void Update(string userId, string resourceId, RightType rightType, Permission permission)
        {
            var userRight = (await Get(userId, resourceId, rightType))
                .ThrowObjectNotFoundExceptionIfNull($"Users rights not found: '{userId}' '{resourceId}'");

            userRight.Rights = permission;

            await _dbContext.SaveAsync(userRight);
        }

        public async Task<UserRight> Get(string userId, string resourceId, RightType type)
        {
            userId.ThrowArgumentExceptionIfNull(nameof(userId), "Cannot be empty");
            userId.ThrowArgumentExceptionIfNull(nameof(resourceId), "Cannot be empty");
            
            return await Get<UserRight>(userId, resourceId, type);
        }

        public async Task<IEnumerable<UserRight>> Get(string userId, string resourceId)
        {
            return await Get<UserRight>(userId, resourceId);
        }

        public async Task RemoveRight(string userId, string resourceId, RightType type)
        {
            userId.ThrowInvalidDataExceptionIfNull("User cannot be empty");
            resourceId.ThrowInvalidDataExceptionIfNullOrWhiteSpace("Resource id must be valid");

            // KLUDGE: not sure how to do this with one query.

            await _dbContext.Delete<UserRight>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId),
                new ScanCondition(nameof(UserRight.Type), ScanOperator.Equal, type)
            });

            Log.TraceFormat(
                "Remove right: user {0} to [{1:G},{2}]",
                userId,
                type,
                resourceId);
        }

        public async Task RemoveRight(string userId, string resourceId)
        {
            userId.ThrowInvalidDataExceptionIfNull("User cannot be empty");
            resourceId.ThrowInvalidDataExceptionIfNullOrWhiteSpace("Resource id must be valid");

            await _dbContext.Delete<UserRight>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId)
            });

            Log.TraceFormat(
                "Remove right: user {0} to [{1:G}, ALL]",
                userId,
                resourceId);
        }

        private async Task<IEnumerable<UserInheritRight>> GetInheritRights(
            RightType resourceType,
            string resourceId,
            RightType inheritType)
        {
            return await _dbContext.Where<UserInheritRight>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserInheritRight.Type), ScanOperator.Equal, resourceType),
                new ScanCondition(nameof(UserInheritRight.ResourceId), ScanOperator.Equal, resourceId),
                new ScanCondition(nameof(UserInheritRight.InheritType), ScanOperator.Equal, inheritType)
            });
        }

        private async Task<UserInheritRight> GetInherit(string userId, string resourceId, RightType inheritType)
        {
            return await Get<UserInheritRight>(userId, resourceId, inheritType);
        }

        private async Task<T> Get<T>(string userId, string resourceId, RightType type) where T : class
        {
            return await _dbContext.SingleOrDefault<T>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId),
                new ScanCondition(nameof(UserRight.Type), ScanOperator.Equal, type)
            });
        }

        private async Task<IEnumerable<T>> Get<T>(string userId, string resourceId) where T : class
        {
            return await _dbContext.Where<T>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId)
            });
        }
    }
}