using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TagStore : ITagStore
    {
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;

        public TagStore(IDynamoDBContext context, IIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<string> Create(TagCreateData createData)
        {
            var tags = await _context.Where<Tag>(nameof(Tag.Name), createData.Name);

            var tag = tags.FirstOrDefault();

            // only create a new tag, if it doesn't already exist
            if (tag.IsNotNull() && !tag.Id.IsNullOrWhitespace())
            {
                return tag.Id;
            }

            var now = DateTime.UtcNow;
            var create = new Tag
            {
                Id = _idGenerator.New(),
                Name = createData.Name,
                // initialise the counter (note: adding a tag to the global set is not the same adding to a todo)
                CreatedAt = now,
            };

            await _context.SaveAsync(create);

            return create.Id;
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

        private async Task Update(string title, Action<Tag> updater)
        {
            var tag = await Get(title)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(tag);

            await _context.SaveAsync(tag);
        }
    }
}