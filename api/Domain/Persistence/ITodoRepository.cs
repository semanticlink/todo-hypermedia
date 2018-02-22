using System;
using System.Collections.Generic;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoRepository
    {
        IEnumerable<Todo> GetAll();
        string Create(TodoCreateData todo);
        Todo Get(string id);
        void Update(string todoId, Action<Todo> updater);
        void Delete(string todoId);
    }
}