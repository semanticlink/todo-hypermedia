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
            RightType rightType,
            Permission permission)
        {
            var userRight = await Get(userId, resourceId);

            if (userRight.IsNotNull())
            {
                return userRight.Id;
            }

            var id = IdGenerator.New();

            userRight = new UserRight
            {
                Id = id,
                UserId = userId,
                ResourceId = resourceId,
                Rights = permission,
                Type = rightType
            };

            await _dbContext.SaveAsync(userRight);

            return id;
        }

        public async Task<string> CreateInherit(
            RightType inheritType,
            string userId,
            string resourceId,
            RightType rightType,
            Permission permission)
        {
            var inheritRight = await GetInherit(userId, resourceId);

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
            
        }

        public async void Update(string userId, string resourceId, Permission permission)
        {
            var userRight = (await Get(userId, resourceId))
                .ThrowObjectNotFoundExceptionIfNull($"Users rights not found: '{userId}' '{resourceId}'");

            userRight.Rights = permission;

            await _dbContext.SaveAsync(userRight);
        }


        public async Task<UserRight> Get(string userId, string resourceId)
        {
            return await Get<UserRight>(userId, resourceId);
        }

        private async Task<UserInheritRight> GetInherit(string userId, string resourceId)
        {
            return await Get<UserInheritRight>(userId, resourceId);
        }

        private async Task<T> Get<T>(string userId, string resourceId) where T : class
        {
            return await _dbContext.SingleOrDefault<T>(new List<ScanCondition>
            {
                new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId),
                new ScanCondition(nameof(UserRight.ResourceId), ScanOperator.Equal, resourceId)
            });
        }
    }

    public class InheritForm
    {
        /// <summary>
        ///     The specific right type for the resource. Note: there can be multiple of these per resourceId
        /// </summary>
        public RightType Type { get; set; }
        
        /// <summary>
        ///     The id (ie database) of resource (eg tenant, user, todo, tag, comment
        /// </summary>
        public string ResourceId { get; set; }
        
        /// <summary>
        ///     The  inherit type on the resource to apply to this resource type
        /// </summary>
        /// <example>
        ///    InheritTypes = new []
        ///     {
        ///         RightType.Todo
        ///         RightType.Comment
        ///     }
        /// </example>
        public IEnumerable<RightType> InheritedTypes { get; set; }
        
        /// <summary>
        ///     The inherit rights to copy from the resource to the new resource
        /// </summary>
        // public IEnumerable<RightType> CopyInheritTypes { get; set; }
    }
}