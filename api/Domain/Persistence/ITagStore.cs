using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITagStore
    {
        Task<string> Create(TagCreateData createData);
        Task<Tag> Get(string id);
        Task<IEnumerable<Tag>> Get(List<string> id);
        Task<IEnumerable<Tag>> GetAll();
        Task IncrementCountOnTag(string id);
        Task DecrementCountOnTag(string id);
        Task Delete(string id);
    }
}