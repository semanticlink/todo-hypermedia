using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Models;

namespace Infrastructure.Db
{
    public class TodoStore
    {
        private readonly IAmazonDynamoDB client;
        private readonly IDynamoDBContext context;

        public const string TableName = "Todo";
        private const string HashKey = "TodoId";

        public TodoStore(IAmazonDynamoDB client, IDynamoDBContext context)
        {
            this.client = client;
            this.context = context;
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
            Console.WriteLine("Sending request to build Widgets table...");
            try
            {
                var result = await client.CreateTableAsync(request);
                Console.WriteLine("Table created.");
                return result.TableDescription;
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                Console.WriteLine("Table already exists. Fetching description...");
                var result = await client.DescribeTableAsync(TableName);
                return result.Table;
            }
        }

        public async Task<string> Create(TodoCreateData todo)
        {
            var id = Guid.NewGuid().ToString();

            var create = new Todo
            {
                Id = id,
                Name = todo.Name,
                Completed = todo.Completed,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow
            };

            await context.SaveAsync<Todo>(create);

            return id;
        }

        public async Task<Todo> GetById(string id)
        {
            List<ScanCondition> conditions =
                new List<ScanCondition> {new ScanCondition(HashKey, ScanOperator.Equal, id)};
            var allDocs = await context.ScanAsync<Todo>(conditions).GetRemainingAsync();
            return allDocs.FirstOrDefault();
        }
    }
}