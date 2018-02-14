using System;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Utils;

namespace TodoApi.Db
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoContext _context;

        public TodoRepository(TodoContext context)
        {
            _context = context;
        }

        public IEnumerable<Todo> GetAll()
        {
            return _context.TodoItems;
        }

        public long Create(TodoCreateData todo)
        {
            var create = new Todo
            {
                Name = todo.Name,
                Completed = todo.Completed,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow
            };

            _context.TodoItems.Add(create);
            _context.SaveChanges();

            return create.Id;
        }

        public Todo Get(long id)
        {
            return _context.TodoItems.SingleOrDefault(x => x.Id == id);
        }

        public void Update(long todoId, Action<Todo> updater)
        {
            var todo = _context
                .TodoItems
                .SingleOrDefault(t => t.Id == todoId)
                .ThrowObjectNotFoundExceptionIfNull("Todo not found");

            updater(todo);

            todo.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }

        public void Delete(long todoId)
        {
            var todo = _context
                .TodoItems
                .SingleOrDefault(t => t.Id == todoId)
                .ThrowObjectNotFoundExceptionIfNull("Todo not found");

            _context.TodoItems.Remove(todo);
            _context.SaveChanges();
        }
    }
}