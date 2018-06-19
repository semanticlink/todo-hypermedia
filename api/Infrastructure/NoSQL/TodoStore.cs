using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TodoStore : ITodoStore
    {
        private readonly IDynamoDBContext _context;

        public TodoStore(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<string> Create(TodoCreateData todo)
        {
            var id = Guid.NewGuid().ToString();

            var create = new Todo
            {
                Id = id,
                Name = todo.Name,
                State = todo.State,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow,
                // TODO: validation/cross checking of tag references
                Tags = todo.Tags
            };

            await _context.SaveAsync(create);

            return id;
        }

        public async Task<Todo> Get(string id)
        {
            return await _context.WhereById<Todo>(id);
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context.Where<Todo>();
        }


        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(todo);
            todo.Id = id;
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(todo);
        }

        public async Task UpdateTag(string id, string tagId)
        {
            await Update(id, todo =>
            {
                if (todo.Tags.IsNull())
                {
                    todo.Tags = new List<string>();
                }

                todo.Tags.Add(tagId);
            });
        }

        public async Task Delete(string id)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(todo);
        }
    }
}