using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITagStore
    {
        Task<string> Create(
            string ownerId,
            string contextResourceId,
            TagCreateData createData,
            Permission callerRights,
            IDictionary<RightType, Permission> collectionCallerRights);

        Task<Tag> Get(string id);
        Task<IEnumerable<Tag>> Get(List<string> id);
        Task<IEnumerable<Tag>> GetAll();        
    }
}