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
        private readonly IUserRightStore _userRightStore;
        private readonly IIdGenerator _idGenerator;
        private readonly string _userId;

        public TodoStore(IDynamoDBContext context, IUserRightStore userRightStore, User user, IIdGenerator idGenerator)
        {
            _context = context;
            _userRightStore = userRightStore;
            _idGenerator = idGenerator;
            _userId = user.Id;
        }

        public async Task<string> Create(TodoCreateData data)
        {
            var todo = new Todo
            {
                Id = _idGenerator.New(),
                Name = data.Name,
                State = data.State,
                Due = data.Due,
                CreatedBy = _userId,
                CreatedAt = DateTime.UtcNow,
                // TODO: validation/cross checking of tag references
                Tags = data.Tags
            };

            await _context.SaveAsync(todo);

            Log.DebugFormat("New todo {0} by user {1}", todo.Id, _userId);

            return todo.Id;
        }

        public async Task<string> Create(
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
                !_userId.IsNullOrWhitespace() ? _userId : contextResourceId,
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


        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            var originalTags = new List<string>(todo.Tags); // clone tags to compare with later

            updater(todo);

            // no messing with the ID allowed
            todo.Id = id;
            // no messing with the update time allowed
            todo.UpdatedAt = DateTime.UtcNow;

            var intersect = originalTags.Intersect(todo.Tags).ToList();
            var toAdd = todo.Tags.Except(intersect).ToList();
            var toRemove = originalTags.Except(intersect).ToList();

            // if tags have been removed, it looks like you can't hand
            // though an empty list but rather need to null it.
            // TODO: check this is true
            todo.Tags = !todo.Tags.IsNullOrEmpty() ? todo.Tags : null;

            await _context.SaveAsync(todo);

            // deal with changed tags user rights
            toAdd.ForEach(addRight => _userRightStore.SetRight(_userId, id, RightType.Tag, Permission.Get));
            toRemove.ForEach(removeRight => _userRightStore.RemoveRight(_userId, id, RightType.Tag));
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
                if (!todo.Tags.Contains(tagId))
                {
                    todo.Tags.Add(tagId);
                }
            });
        }

        public async Task Delete(string id)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();


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
    }
}