using System;
using System.Collections.Generic;
using TodoApi.Models;

namespace TodoApi.Db
{
    public interface ITodoRepository
    {
        IEnumerable<Todo> GetAll();
        long Create(TodoCreateData todo);
        Todo Get(long id);
        void Update(long todoId, Action<Todo> updater);
        void Delete(long todoId);
    }
}