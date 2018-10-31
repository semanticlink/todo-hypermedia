using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoStore
    {
        Task<string> Create(
            string ownerId, 
            string contextResourceId,
            TodoCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights);

        /// <summary>
        ///     Retrieve a <see cref="Todo"/> based on its id
        /// </summary>
        Task<Todo> Get(string id);

        /// <summary>
        ///     Retrieve a <see cref="Todo"/> based on its id and the presence of a tag.
        /// </summary>
        Task<Todo> GetByIdAndTag(string id, string tagId);

        /// <summary>
        ///     Retrieve a full set of tags (ie the global collection) regardless of <see cref="Tenant"/>
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Todo>> GetAll();

        /// <summary>
        ///     Retrieve a full set of todos by <see cref="User"/>
        /// </summary>
        Task<IEnumerable<Todo>> GetByUser();

        /// <summary>
        ///     Update details of a <see cref="Todo"/> that includes checking if the <see cref="Todo.Tags"/> have changed
        /// </summary>
        Task Update(string id, Action<Todo> updater);

        /// <summary>
        ///     Add an existing <see cref="Tag"/> to a <see cref="Todo"/>. 
        /// </summary>
        /// <param name="id"><see cref="Todo.Id"/> of the todo that has the tags</param>
        /// <param name="tagId"><see cref="Tag.Id"/> of the tag to be added</param>
        /// <param name="add">Callback on the <see cref="Tag.Id"/>. The default action is to increment the <see cref="Tag.Count"/> on the global collection of tags</param>
        Task AddTag(string id, string tagId, Action<string> add = null);

        /// <summary>
        ///     Remove a <see cref="Todo"/> from the collection for and decrement <see cref="Tag.Count"/> on the global
        ///     collection
        /// </summary>
        Task Delete(string id);

        /// <summary>
        ///     Remove a <see cref="Tag"/> from a <see cref="Todo"/>. 
        /// </summary>
        /// <param name="id"><see cref="Todo.Id"/> of the todo that has the tags</param>
        /// <param name="tagId"><see cref="Tag.Id"/> of the tag to be deleted</param>
        /// <param name="remove">Callback on the <see cref="Tag.Id"/>. The default action is to decrement the <see cref="Tag.Count"/> on the global collection of tags</param>
        Task DeleteTag(string id, string tagId, Action<string> remove = null);

        /// <summary>
        ///     Retrieve a list of todo based on a <see cref="Tag"/>.
        /// </summary>
        /// <param name="tagId"><see cref="Tag.Id"/> of the tag to be searched across all todos</param>
        Task<IEnumerable<Todo>> GetByTag(string tagId);

        /// <summary>
        ///     Retrieve a list of todo based on a <see cref="TodoList"/>
        /// </summary>
        /// <param name="todoListId"><see cref="TodoList.Id"/> of the tag to be searched across all todos</param></param>
        Task<IEnumerable<Todo>> GetByParent(string todoListId);

        Task DeleteByParent(string parentId);
    }
}