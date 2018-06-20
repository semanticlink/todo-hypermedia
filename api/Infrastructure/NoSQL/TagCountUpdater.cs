using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;

namespace Infrastructure.NoSQL
{
    /// <summary>
    ///     Helps maintain the <see cref="Tag"/> <see cref="Tag.Count"/> references in the global collection
    /// </summary>
    public class TagCountUpdater
    {
        private readonly ITagStore _tagStore;

        /// <summary>
        ///     Callback that increments the counter <see cref="ITagStore.IncrementCountOnTag"/>  on the tag on the global tag collection
        /// </summary>
        private Action<string> Increment
        {
            get { return async id => await _tagStore.IncrementCountOnTag(id); }
        }

        /// <summary>
        ///     Callback that decrements the counter <see cref="ITagStore.DecrementCountOnTag"/> on the tag on the global tag collection
        /// </summary>
        private Action<string> Decrement
        {
            get { return async id => await _tagStore.DecrementCountOnTag(id); }
        }

        public TagCountUpdater(ITagStore tagStore)
        {
            _tagStore = tagStore;
        }

        /// <summary>
        ///     Based on the difference between two sets of ids, makes callbacks to remove or add counters
        ///     on the global tag collection
        /// </summary>
        /// <param name="initial">List of tag <see cref="Tag.Id"/>s (old state)</param>
        /// <param name="updated">List of tag <see cref="Tag.Id"/>s (new state)</param>
        /// <param name="add">Optional callback that has a default callack <see cref="Increment"/></param>
        /// <param name="remove">Optional callback that has a default callack <see cref="Decrement"/></param>
        public Task Update(
            IEnumerable<string> initial,
            IEnumerable<string> updated,
            Action<string> add = null,
            Action<string> remove = null)
        {
            return Compare(initial, updated, add ?? Increment, remove ?? Decrement);
        }

        /// <summary>
        ///     Helper method. Update for newly created tag lists
        /// </summary>
        public Task Update(
            IEnumerable<string> updated,
            Action<string> add = null,
            Action<string> remove = null)
        {
            return Compare(new List<string>(), updated, add, remove);
        }

        /// <summary>
        ///     Based on the difference between two sets of ids, makes callbacks to remove or add.
        /// </summary>
        /// <remarks>
        ///    This method lives by itself to be testable. 
        /// </remarks>
        /// <param name="initial">List of tag <see cref="Tag.Id"/>s (old state)</param>
        /// <param name="updated">List of tag <see cref="Tag.Id"/>s (new state)</param>
        /// <param name="add">Callback when an id has been added to the set</param>
        /// <param name="remove">Callback when an id is no longer part of the set</param>
        public static async Task Compare(
            IEnumerable<string> initial,
            IEnumerable<string> updated,
            Action<string> add,
            Action<string> remove)
        {
            // ensure that null lists are empty
            initial = initial ?? new List<string>();
            updated = updated ?? new List<string>();

            var cached = initial.ToList();
            var obj = updated.ToList();

            var removedAndChanged = cached.Except(obj).ToList();
            var newAndChanged = obj.Except(cached).ToList();

            var tasks = newAndChanged
                .Select(async id => add(id))
                .Concat(removedAndChanged
                    .Select(async id => remove(id)))
                .ToArray();

            await Task.WhenAll(tasks);
        }
    }
}