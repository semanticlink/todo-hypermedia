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

        public const string TableName = TableNameConstants.Todo;
        private const string HashKey = HashKeyConstants.DEFAULT;

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
                Completed = todo.Completed,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync(create);

            return id;
        }

        public async Task<Todo> Get(string id)
        {
            return (await _context
                    .ScanAsync<Todo>(new List<ScanCondition>
                    {
                        new ScanCondition(HashKey, ScanOperator.Equal, id)
                    })
                    .GetRemainingAsync())
                .SingleOrDefault();
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context
                .ScanAsync<Todo>(new List<ScanCondition>())
                .GetRemainingAsync();
        }

        public async Task Update(string todoId, Action<Todo> updater)
        {
            var todo = await Get(todoId)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(todo);
            todo.Id = todoId;

            await _context.SaveAsync(todo);
        }

        public async Task Delete(string id)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(todo);
        }
    }
}