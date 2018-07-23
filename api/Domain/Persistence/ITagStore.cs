using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITagStore
    {
        [Obsolete("Use create with permission setting")]
        Task<string> Create(TagCreateData createData);

        Task<string> Create(
            string ownerId,
            string contextResourceId,
            TagCreateData createData,
            Permission callerRights,
            IDictionary<RightType, Permission> collectionCallerRights);

        Task<Tag> Get(string id);
        Task<IEnumerable<Tag>> Get(List<string> id);
        Task<IEnumerable<Tag>> GetAll();
        
        [Obsolete("Tags are immutable-test classes must delete directly on context")]
        Task Delete(string id);
    }
}