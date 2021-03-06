﻿using System;
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
    public class TodoStore : ITodoStore
    {
        private ILogger Log { get; }
        private readonly IDynamoDBContext _context;
        private readonly string _creatorId;
        private readonly IIdGenerator _idGenerator;
        private readonly ITagStore _tagStore;
        private readonly ITenantStore _tenantStore;
        private readonly IUserRightStore _userRightStore;

        public TodoStore(User creator,
            IDynamoDBContext context,
            IIdGenerator idGenerator,
            IUserRightStore userRightStore,
            ITagStore tagStore,
            ITenantStore tenantStore,
            ILogger<TodoStore> log)
        {
            Log = log;
            _context = context;
            _userRightStore = userRightStore;
            _idGenerator = idGenerator;
            _tagStore = tagStore;
            _tenantStore = tenantStore;
            _creatorId = creator.Id;
        }

        private async Task<string> Create(TodoCreateData data)
        {
            var todo = new Todo
            {
                // lists have the Id == Parent
                // items have separate Id and Parent ids
                Id = _idGenerator.New(),

                Parent = data.Parent
                    .ThrowConfigurationErrorsExceptionIfNullOrWhiteSpace("Parent cannot be empty"),

                Type = data.Type,

                Name = data.Name,
                State = data.State,
                Due = data.Due,
                CreatedBy = _creatorId,
                CreatedAt = DateTime.UtcNow,
                // TODO: validation/cross checking of tag references
                Tags = data.Tags
            };

            await _context.SaveAsync(todo);

            Log.DebugFormat("New todo {0} for user {1} by user {1}", todo.Id, _creatorId);

            return todo.Id;
        }

        public async Task<string> Create(
            string ownerId,
            string contextResourceId,
            TodoCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            var todoId = await Create(data);

            // create a security hole, if the user isn't injected then make the creator the parent resource
            // which in this case is a user.
            // KLUDGE : take out. This is only here because of seed data
            await _userRightStore.CreateRights(
                ownerId,
                todoId,
                RightType.Todo.MakeCreateRights(callerRights, callerCollectionRights),
                new InheritForm
                {
                    Type = RightType.UserTodoCollection,
                    ResourceId = contextResourceId,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Todo,
                        RightType.TodoCommentCollection,
                        RightType.TodoTagCollection
                    }
                });
            return todoId;
        }


        public async Task<Todo> Get(string id)
        {
            return await _context.WhereById<Todo>(id);
        }

        public async Task<Todo> GetByIdAndTag(string id, string tagId)
        {
            return (await _context.Where<Todo>(new List<ScanCondition>
                {
                    new ScanCondition(HashKeyConstants.Default, ScanOperator.Equal, id),
                    new ScanCondition(nameof(Todo.Tags), ScanOperator.Contains, tagId)
                }))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context.Where<Todo>();
        }

        public async Task<IEnumerable<Todo>> GetByUser()
        {
            // TODO: implement by user
            return await GetAll();
        }

        public async Task<IEnumerable<Todo>> GetByUser(string userId)
        {
            var tenantIds = (await _tenantStore
                    .GetTenantsForUser(userId))
                .Select(x => x.Id);

            return await ByTenantAndUser(tenantIds);
        }

        public async Task<IEnumerable<Todo>> GetByTenantAndUser(string tenantId, string userId)
        {
            var tenantIds = (await _tenantStore
                    .GetTenantsForUser(userId))
                .Where(x => x.Id.Equals(tenantId, StringComparison.InvariantCulture))
                .Select(x => x.Id);

            return await ByTenantAndUser(tenantIds);
        }

        private async Task<IEnumerable<Todo>> ByTenantAndUser(IEnumerable<string> tenantIds)
        {
            var tasks = tenantIds
                .ToList()
                .Select(id =>
                    _context.Where<Todo>(new List<ScanCondition>
                    {
                        new ScanCondition(nameof(Todo.Parent), ScanOperator.Contains, id)
                    }));

            // KLUDGE: inefficient and need to implement Batch Get
            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToList();
        }

        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            var todoListId = todo.Parent;

            var originalTags = todo.Tags.IsNullOrEmpty()
                ? new List<string>()
                : new List<string>(todo.Tags); // clone tags to compare with later

            updater(todo);

            await SetRightsForTodoTag(id, originalTags, todo.Tags);

            // no messing with the IDs allowed
            todo.Id = id;
            todo.Parent = todoListId;

            // no messing with the update time allowed
            todo.UpdatedAt = DateTime.UtcNow;

            // if tags have been removed, it looks like you can't hand
            // though an empty list but rather need to null it.
            // TODO: check this is true
            todo.Tags = !todo.Tags.IsNullOrEmpty() ? todo.Tags : null;

            await _context.SaveAsync(todo);
        }


        /// <summary>
        ///     Add tags to the todo. You are not able to add duplicates.
        /// </summary>
        /// <remarks>
        ///    This implementation has a known problem. Adding multiple tags at once will result in a error. There
        ///     is no handling for optimistic locking. This has been left in to demonstrate the need for sequential
        ///     requests on the client.
        /// </remarks>
        public async Task AddTag(string id, string tagId, Action<string> add = null)
        {
            await Update(id, todo =>
            {
                todo.Tags = todo.Tags ?? new List<string>();

                // do not allow duplicates
                if (!todo.Tags.Contains(tagId)) todo.Tags.Add(tagId);
            });
        }

        /// <summary>
        ///     Delete a todo. 
        /// </summary>
        /// <remarks>
        ///    Currently a simple implementation for a list/item structure. If the todo is a list then delete it
        ///    and all its children other just delete itself because it is an item. 
        /// </remarks>
        public async Task Delete(string id)
        {
            var todo = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            if (todo.Type.Equals(TodoType.List))
            {
                await DeleteByParent(id);
            }

            if (todo.Tags.IsNotNull())
            {
                await Task.WhenAll(todo.Tags.Select(RemoveRightForTag));
            }

            await _userRightStore.RemoveRight(_creatorId, id);

            await _context.DeleteAsync(todo);
        }

        /// <summary>
        ///     Delete a todo children
        /// </summary>
        public async Task DeleteByParent(string parentId)
        {
            (await Get(parentId))
                .ThrowObjectNotFoundExceptionIfNull()
                .ThrowInvalidDataExceptionIf(list => !list.Type.Equals(TodoType.List), "Todo is an item not a list");

            var tasks = (await GetByParent(parentId)).Select(todo => Delete(todo.Id));

            await Task.WhenAll(tasks);

            await _userRightStore.RemoveRight(_creatorId, parentId);
        }

        public async Task DeleteTag(string id, string tagId, Action<string> remove = null)
        {
            await Update(id, todo => todo.Tags?.RemoveAll(tag => tag == tagId));
        }

        public async Task<IEnumerable<Todo>> GetByTag(string tagId)
        {
            return await _context.Where<Todo>(new ScanCondition(nameof(Todo.Tags), ScanOperator.Contains, tagId));
        }

        public async Task<IEnumerable<Todo>> GetByParent(string todoListId)
        {
            return await _context.Where<Todo>(
                new ScanCondition(nameof(Todo.Parent), ScanOperator.Contains, todoListId));
        }

        private async Task SetRightsForTodoTag(string todoId, IList<string> origIds, IList<string> newIds)
        {
            origIds = origIds ?? new List<string>();
            newIds = newIds ?? new List<string>();

            var newSet = newIds.Distinct().ToList();
            var existingSet = (await _tagStore.Get(newSet)).Select(tag => tag.Id).ToList();

            (existingSet.Count == newSet.Count && newSet.Intersect(existingSet).Count() == existingSet.Count)
                .ThrowInvalidDataExceptionIf(x => false,
                    "Some tags do not exist in the global set and must be created first");

            var intersect = origIds.Intersect(newIds).ToList();
            var toAdd = newIds.Except(intersect).ToList();
            var toRemove = origIds.Except(intersect).ToList();

            // deal with changed tags user rights
            await Task.WhenAll(toAdd
                .Select(SetRightForTag)
                .Concat(toRemove.Select(RemoveRightForTag)));
        }

        private async Task SetRightForTag(string tagId)
        {
            await _userRightStore.SetRight(_creatorId, tagId, RightType.Tag, Permission.Get);
        }

        private async Task RemoveRightForTag(string tagId)
        {
            await _userRightStore.RemoveRight(_creatorId, tagId, RightType.Tag);
        }
    }
}