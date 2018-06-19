using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TagStore : ITagStore
    {
        private readonly IDynamoDBContext _context;

        public TagStore(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<string> Create(TagCreateData createData)
        {
            var tags = await _context.Where<Tag>(nameof(Tag.Name), createData.Name);

            var tag = tags.FirstOrDefault();

            if (tag.IsNotNull() && !tag.Id.IsNullOrWhitespace())
            {
                return tag.Id;
            }

            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var create = new Tag
            {
                Id = id,
                Name = createData.Name,
                Count = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.SaveAsync(create);

            return id;
        }


        public async Task IncrementCountOnTag(string id)
        {
            var tag = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            tag.Count++;

            await _context.SaveAsync(tag);
        }

        public async Task DecrementCountOnTag(string id)
        {
            var tag = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            tag.Count--;

            await _context.SaveAsync(tag);
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

            tag.Count++;

            await _context.SaveAsync(tag);
        }
    }
}