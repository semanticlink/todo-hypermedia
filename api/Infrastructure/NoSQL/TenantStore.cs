using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Models;
using Domain.Persistence;

namespace Infrastructure.Db
{
    public class TenantStore : ITenantStore
    {
        private readonly IAmazonDynamoDB _client;
        private readonly IDynamoDBContext _context;

        public const string TableName = "Tenant";
        private const string HashKey = "Id";

        public TenantStore(IAmazonDynamoDB client, IDynamoDBContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<TableDescription> BuildOrDescribeTable()
        {
            var request = new CreateTableRequest(
                tableName: TableName,
                keySchema: new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = HashKey,
                        KeyType = KeyType.HASH
                    }
                },
                attributeDefinitions: new List<AttributeDefinition>
                {
                    new AttributeDefinition()
                    {
                        AttributeName = HashKey,
                        AttributeType = ScalarAttributeType.S
                    }
                },
                provisionedThroughput: new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            );
            Console.WriteLine("Sending request to build table...");
            try
            {
                var result = await _client.CreateTableAsync(request);
                Console.WriteLine("Table created.");
                return result.TableDescription;
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                Console.WriteLine("Table already exists. Fetching description...");
                var result = await _client.DescribeTableAsync(TableName);
                Console.WriteLine($"Using table: {result.Table.TableName} ");
                return result.Table;
            }
        }

        public async Task<string> Create(TenantCreateData todo)
        {
            var id = Guid.NewGuid().ToString();

            var create = new Tenant
            {
                Id = id,
                Name = todo.Name,
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync<Tenant>(create);

            return id;
        }

        public async Task<Tenant> GetById(string id)
        {
            List<ScanCondition> conditions =
                new List<ScanCondition> {new ScanCondition(HashKey, ScanOperator.Equal, id)};
            var allDocs = await _context.ScanAsync<Tenant>(conditions).GetRemainingAsync();
            return allDocs.FirstOrDefault();
        }

        public async Task<Tenant> GetByCode(string code)
        {
            List<ScanCondition> conditions =
                new List<ScanCondition> {new ScanCondition("Code", ScanOperator.Equal, code)};
            var doc = await _context.ScanAsync<Tenant>(conditions).GetRemainingAsync();
            return doc.SingleOrDefault();
        }

        public async Task<IEnumerable<Tenant>> GetTenantsForUser(string id)
        {
            List<ScanCondition> conditions =
                new List<ScanCondition> {new ScanCondition(HashKey, ScanOperator.Equal, id)};
            return await _context.ScanAsync<Tenant>(conditions).GetRemainingAsync();
        }
    }
}