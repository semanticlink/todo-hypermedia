using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TodoListStore : ITodoListStore
    {
        private ILogger Log { get; }
        private readonly IDynamoDBContext _context;
        private readonly string _creatorId;
        private readonly IIdGenerator _idGenerator;
        private readonly ITenantStore _tenantStore;
        private readonly ITodoStore _todoStore;
        private readonly IUserRightStore _userRightStore;

        public TodoListStore(User creator,
            IDynamoDBContext context,
            IIdGenerator idGenerator,
            IUserRightStore userRightStore,
            ITenantStore tenantStore,
            ITodoStore todoStore,
            ILogger<TodoListStore> log)
        {
            Log = log;
            _context = context;
            _userRightStore = userRightStore;
            _idGenerator = idGenerator;
            _tenantStore = tenantStore;
            _todoStore = todoStore;
            _creatorId = creator.Id;
        }

        private async Task<string> Create(TodoListCreateData data)
        {
            var todoList = new TodoList
            {
                Id = _idGenerator.New(),
                Parent = data.Parent
                    .ThrowConfigurationErrorsExceptionIfNullOrWhiteSpace("Tenant cannot be empty"),
                Name = data.Name,
                CreatedBy = _creatorId,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.SaveAsync(todoList);

            Log.DebugFormat("New todo list {0} for user {1} by user {1}", todoList.Id, _creatorId);

            return todoList.Id;
        }

        public async Task<string> Create(
            string ownerId,
            string contextResourceId,
            TodoListCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            var id = await Create(data);

            // create a security hole, if the user isn't injected then make the creator the parent resource
            // which in this case is a user.
            // KLUDGE : take out. This is only here because of seed data
            await _userRightStore.CreateRights(
                ownerId,
                id,
                RightType.Todo.MakeCreateRights(callerRights, callerCollectionRights),
                new InheritForm
                {
                    Type = RightType.UserTodoCollection,
                    ResourceId = contextResourceId,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Todo,
/*
                        // todo list does have comments or tags (but might)
                        RightType.TodoCommentCollection,
                        RightType.TodoTagCollection
*/
                    }
                });


            return id;
        }

        public async Task<TodoList> Get(string id)
        {
            return await _context.WhereById<TodoList>(id);
        }

        public async Task<IEnumerable<TodoList>> GetByUser(string userId)
        {
            var tenantIds = (await _tenantStore
                    .GetTenantsForUser(userId))
                .Select(x => x.Id);

            return await ByTenantAndUser(tenantIds);
        }

        public async Task<IEnumerable<TodoList>> GetByTenantAndUser(string tenantId, string userId)
        {
            var tenantIds = (await _tenantStore
                    .GetTenantsForUser(userId))
                .Where(x => x.Id.Equals(tenantId, StringComparison.InvariantCulture))
                .Select(x => x.Id);

            return await ByTenantAndUser(tenantIds);
        }

        private async Task<IEnumerable<TodoList>> ByTenantAndUser(IEnumerable<string> tenantIds)
        {
            var tasks = tenantIds
                .ToList()
                .Select(id =>
                    _context.Where<TodoList>(new List<ScanCondition>
                    {
                        new ScanCondition(nameof(TodoList.Parent), ScanOperator.Contains, id)
                    }));

            // KLUDGE: inefficient and need to implement Batch Get
            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToList();
        }

        public async Task Delete(string id)
        {
            // remove list context    
            var todoList = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            await _todoStore.DeleteByParent(id);

            await _userRightStore.RemoveRight(_creatorId, id);
            await _context.DeleteAsync(todoList);
        }

        public async Task Update(string id, Action<TodoList> updater)
        {
            var todo = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            var tenantId = todo.Parent;

            updater(todo);

            // no messing with the IDs allowed
            todo.Id = id;
            todo.Parent = tenantId;

            // no messing with the update time allowed
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(todo);
        }
    }
}