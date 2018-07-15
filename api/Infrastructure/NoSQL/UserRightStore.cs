using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserRightStore : IUserRightStore
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IDynamoDBContext _dbContext;

        public UserRightStore(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> SetRight(
            string userId,
            string resourceId,
            RightType type,
            Permission permission)
        {
            var userRight = await Get(userId, resourceId, type);

            if (userRight.IsNotNull())
            {
                // TODO: is this actually an update?
                return userRight.Id;
            }

            var id = IdGenerator.New();

            userRight = new UserRight
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permission,
                Type = type
            };

            Log.TraceFormat("Add right: user {0} to [{1:G},{2}] => rights {3:X} ({3:G})", userId, type, resourceId,
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

            if (inheritRight.IsNotNull())
            {
                return inheritRight.Id;
            }

            var id = IdGenerator.New();

            inheritRight = new UserInheritRight
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permission,
                Type = rightType,
                InheritType = inheritType
            };

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
            foreach (var right in granted)
            {
                await SetRight(userId, resourceId, right.Key, right.Value);
            }

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
                {
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
        }

        public async void Update(string userId, string resourceId, RightType rightType, Permission permission)
        {
            var userRight = (await Get(userId, resourceId, rightType))
                .ThrowObjectNotFoundExceptionIfNull($"Users rights not found: '{userId}' '{resourceId}'");

            userRight.Rights = permission;

            await _dbContext.SaveAsync(userRight);
        }

        public async Task<UserRight> Get(string userId, string resourceId, RightType rightType)
        {
            return await Get<UserRight>(userId, resourceId, rightType);
        }

        public async Task<IEnumerable<UserRight>> Get(string userId, string resourceId)
        {
            return await Get<UserRight>(userId, resourceId);
        }

        private async Task<IEnumerable<UserInheritRight>> GetInheritRights(RightType resourceType, string resourceId,
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

        private async Task<T> Get<T>(string userId, string resourceId, RightType rightType) where T : class
        {
            return await _dbContext.SingleOrDefault<T>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId),
                new ScanCondition(nameof(UserRight.Type), ScanOperator.Equal, rightType)
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