using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITenantStore
    {
        Task<TableDescription> BuildOrDescribeTable();
        Task<string> Create(TenantCreateData todo);
        Task<Tenant> GetById(string id);
        Task<Tenant> GetByCode(string id);
        Task<IEnumerable<Tenant>> GetTenantsForUser(string id);
    }
}