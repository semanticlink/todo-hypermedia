using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TodoStore : ITodoStore
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IDynamoDBContext _context;
        private readonly string _creatorId;
        private readonly IIdGenerator _idGenerator;
        private readonly ITagStore _tagStore;
        private readonly IUserRightStore _userRightStore;

        public TodoStore(
            User creator,
            IDynamoDBContext context,
            IIdGenerator idGenerator,
            IUserRightStore userRightStore,
            ITagStore tagStore)
        {
            _context = context;
            _userRightStore = userRightStore;
            _idGenerator = idGenerator;
            _tagStore = tagStore;
            _creatorId = creator.Id;
        }

        private async Task<string> Create(TodoCreateData data)
        {
            var todo = new Todo
            {
                Id = _idGenerator.New(),
                Tenant = data.Tenant
                    .ThrowConfigurationErrorsExceptionIfNullOrWhiteSpace("Tenant cannot be empty"),
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

        public async Task<IEnumerable<Todo>> GetByTenant(string tenantId)
        {
            return await _context.Where<Todo>(new List<ScanCondition>
            {
                new ScanCondition(nameof(Todo.Tenant), ScanOperator.Equal, tenantId)
            });
        }


        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            var tenantId = todo.Tenant;

            var originalTags = todo.Tags.IsNullOrEmpty()
                ? new List<string>()
                : new List<string>(todo.Tags); // clone tags to compare with later

            updater(todo);

            await SetRightsForTodoTag(id, originalTags, todo.Tags);

            // no messing with the IDs allowed
            todo.Id = id;
            todo.Tenant = tenantId;

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
        public async Task AddTag(string id, string tagId, Action<string> add = null)
        {
            await Update(id, todo =>
            {
                todo.Tags = todo.Tags ?? new List<string>();

                // do not allow duplicates
                if (!todo.Tags.Contains(tagId)) todo.Tags.Add(tagId);
            });
        }

        public async Task Delete(string id)
        {
            var todo = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            if (todo.Tags.IsNotNull())
            {
                await Task.WhenAll(todo.Tags.Select(RemoveRightForTag));
            }

            await _userRightStore.RemoveRight(_creatorId, id);

            await _context.DeleteAsync(todo);
        }

        public async Task DeleteTag(string id, string tagId, Action<string> remove = null)
        {
            await Update(id, todo => todo.Tags?.RemoveAll(tag => tag == tagId));
        }

        public async Task<IEnumerable<Todo>> GetByTag(string tagId)
        {
            return await _context.Where<Todo>(new ScanCondition(nameof(Todo.Tags), ScanOperator.Contains, tagId));
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