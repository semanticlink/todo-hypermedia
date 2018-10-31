using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoListStore
    {
        Task<string> Create(
            string ownerId,
            string contextResourceId,
            TodoListCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);

        /// <summary>
        ///     Retrieve a <see cref="TodoList"/> based on its id
        /// </summary>
        Task<TodoList> Get(string id);

        /// <summary>
        ///     Retrieve a full set of todo lists by authenticated <see cref="User"/>
        /// </summary>
        /// <param name="userId"></param>
        Task<IEnumerable<TodoList>> GetByUser(string userId);

        Task<IEnumerable<TodoList>> GetByTenantAndUser(string tenantId, string userId);
        
        /// <summary>
        ///     Deletes the todo list, its access rights and child todo
        /// </summary>
        Task Delete(string id);

        Task Update(string id, Action<TodoList> updater);
    }
}