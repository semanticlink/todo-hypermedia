using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.Db
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

        public string Create(TodoCreateData todo)
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

        public Todo Get(string id)
        {
            return _context.TodoItems.SingleOrDefault(x => x.Id == id);
        }

        public void Update(string todoId, Action<Todo> updater)
        {
            var todo = _context
                .TodoItems
                .SingleOrDefault(t => t.Id == todoId)
                .ThrowObjectNotFoundExceptionIfNull("Todo not found");

            updater(todo);

            todo.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }

        public void Delete(string todoId)
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