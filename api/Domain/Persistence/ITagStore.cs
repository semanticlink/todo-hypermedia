using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITagStore
    {
        [Obsolete]
        Task<string> Create(TagCreateData createData);

        Task<string> Create(
            string creatorId,
            string contextResourceId,
            TagCreateData createData,
            Permission callerRights,
            IDictionary<RightType, Permission> collectionCallerRights);

        Task<Tag> Get(string id);
        Task<IEnumerable<Tag>> Get(List<string> id);
        Task<IEnumerable<Tag>> GetAll();
        Task Delete(string id);
    }
}