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
        Task UpdateTag(string id, string tagId, Action<string> add = null);
        Task Delete(string id);
        Task DeleteTag(string id, string tagId,  Action<string> remove = null);
    }
}