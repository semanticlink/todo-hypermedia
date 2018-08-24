using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TagStore : ITagStore
    {
        private ILogger Log { get; }
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly string _userId;
        private readonly IUserRightStore _userRightStore;

        public TagStore(
            User creator,
            IDynamoDBContext context,
            IIdGenerator idGenerator,
            IUserRightStore userRightStore,
            ILogger<TagStore> log)
        {
            Log = log;
            _context = context;
            _idGenerator = idGenerator;
            _userRightStore = userRightStore;
            _userId = creator.Id;
        }

        public async Task<string> Create(
            string ownerId,
            string contextResourceId,
            TagCreateData createData,
            Permission callerRights,
            IDictionary<RightType, Permission> collectionCallerRights)
        {
            var tagId = await Create(createData);

            await _userRightStore.CreateRights(
                ownerId,
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

        public async Task<IEnumerable<Tag>> Get(List<string> ids)
        {
            return ids.IsNullOrEmpty()
                ? new List<Tag>()
                : await _context.WhereByIds<Tag>(ids);
        }

        public async Task<IEnumerable<Tag>> GetAll()
        {
            return await _context.Where<Tag>();
        }

        private async Task<string> Create(TagCreateData data)
        {
            var tags = await _context.Where<Tag>(nameof(Tag.Name), data.Name);

            var tag = tags.FirstOrDefault();

            // only create a new tag, if it doesn't already exist
            if (tag != null && !tag.Id.IsNullOrWhitespace()) return tag.Id;

            var create = new Tag
            {
                Id = _idGenerator.New(),
                Name = data.Name,
                CreatedBy = _userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync(create);

            Log.DebugFormat("New tag {0} created by user {1}", create.Id, _userId);

            return create.Id;
        }

        public async Task Delete(string id)
        {
            var tag = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(tag);
        }


        private async Task Update(string title, Action<Tag> updater)
        {
            var tag = (await Get(title))
                .ThrowObjectNotFoundExceptionIfNull();

            updater(tag);

            await _context.SaveAsync(tag);
        }
    }
}