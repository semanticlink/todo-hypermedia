using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain.Models;
using Domain.Persistence;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TodoStore : ITodoStore
    {
        private readonly IDynamoDBContext _context;

        private readonly Action<string> _increment;
        private readonly Action<string> _decrement;

        public TodoStore(IDynamoDBContext context, ITagStore tagStore)
        {
            _context = context;

            _increment = async id => { await tagStore.IncrementCountOnTag(id); };
            _decrement = async id => { await tagStore.DecrementCountOnTag(id); };
        }

        public async Task<string> Create(TodoCreateData todo)
        {
            var id = Guid.NewGuid().ToString();

            var create = new Todo
            {
                Id = id,
                Name = todo.Name,
                State = todo.State,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow,
                // TODO: validation/cross checking of tag references
                Tags = todo.Tags
            };

            await Task.WhenAll(todo.Tags.Select(async tagId => _increment(tagId)));
            
            await _context.SaveAsync(create);

            return id;
        }

        public async Task<Todo> Get(string id)
        {
            return await _context.WhereById<Todo>(id);
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context.Where<Todo>();
        }


        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();
            
            // TODO:
            // TODO: need to diff the tags and alter the count
            // TODO:

            updater(todo);
            todo.Id = id;
            todo.UpdatedAt = DateTime.UtcNow;

            // if tags have been removed, it looks like you can't hand
            // though an empty list but rather need to null it.
            // TODO: check this is true
            if (todo.Tags.IsNullOrEmpty())
            {
                todo.Tags = null;
            }

            await _context.SaveAsync(todo);
        }

        public async Task UpdateTag(string id, string tagId, Action<string> add = null)
        {
            await Update(id, todo =>
            {
                if (todo.Tags.IsNull())
                {
                    todo.Tags = new List<string>();
                }

                todo.Tags.Add(tagId);

                if (add.IsNull())
                {
                    _increment(tagId);
                }
                else
                {
                    add?.Invoke(tagId);
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
            await Update(id, todo =>
            {
                if (!todo.Tags.IsNullOrEmpty())
                {
                    todo.Tags.RemoveAll(tag =>
                    {
                        if (tag == tagId)
                        {
                            if (remove.IsNull())
                            {
                                _decrement(tagId);
                            }
                            else
                            {
                                remove?.Invoke(tagId);
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
            });
        }
    }
}