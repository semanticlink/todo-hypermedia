using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TagStore : ITagStore
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly string _userId;
        private readonly IUserRightStore _userRightStore;

        public TagStore(IDynamoDBContext context, IIdGenerator idGenerator, IUserRightStore userRightStore, User user)
        {
            _context = context;
            _idGenerator = idGenerator;
            _userRightStore = userRightStore;
            _userId = user.Id;
        }

        public async Task<string> Create(TagCreateData createData)
        {
            return await Create(_userId, createData);
        }

        public async Task<string> Create(
            string creatorId,
            string contextResourceId,
            TagCreateData createData,
            Permission callerRights,
            IDictionary<RightType, Permission> collectionCallerRights)
        {
            var tagId = await Create(creatorId, createData);

            await _userRightStore.CreateRights(
                creatorId,
                tagId,
                RightType.Tag.MakeCreateRights(callerRights, collectionCallerRights),
                new InheritForm
                {
                    Type = RightType.RootTagCollection,
                    ResourceId = contextResourceId,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Tag
                    }
                });

            return tagId;
        }


        public async Task<Tag> Get(string id)
        {
            return await _context.FirstOrDefault<Tag>(id);
        }

        public async Task<IEnumerable<Tag>> Get(IList<string> ids)
        {
            return ids.IsNullOrEmpty()
                ? new List<Tag>()
                : await _context.WhereByIds<Tag>(ids);
        }

        public async Task Delete(string id)
        {
            var tag = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(tag);
        }

        public async Task<IEnumerable<Tag>> GetAll()
        {
            return await _context.Where<Tag>();
        }

        private async Task<string> Create(string creatorId, TagCreateData data)
        {
            var tags = await _context.Where<Tag>(nameof(Tag.Name), data.Name);

            var tag = tags.FirstOrDefault();

            // only create a new tag, if it doesn't already exist
            if (tag != null && !tag.Id.IsNullOrWhitespace())
            {
                return tag.Id;
            }

            var create = new Tag
            {
                Id = _idGenerator.New(),
                Name = data.Name,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync(create);

            Log.DebugFormat("New tag {0} created by user {1}", create.Id, _userId);

            return create.Id;
        }

        private async Task Update(string title, Action<Tag> updater)
        {
            var tag = await Get(title)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(tag);

            await _context.SaveAsync(tag);
        }
    }
}