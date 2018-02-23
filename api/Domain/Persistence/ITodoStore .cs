using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoStore
    {
        Task<TableDescription> BuildOrDescribeTable();
        Task<string> Create(TodoCreateData todo);
        Task<Todo> GetById(string id);
    }
}