using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoStore
    {
        Task<string> Create(TodoCreateData todo);
        Task<Todo> Get(string id);
        Task<IEnumerable<Todo>> GetAll();
        Task Update(string id, Action<Todo> updater);
        Task Delete(string id);
    }
}