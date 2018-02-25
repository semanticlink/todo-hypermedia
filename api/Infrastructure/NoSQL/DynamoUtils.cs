using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Infrastructure.NoSQL
{
    public static class DynamoUtils
    {
        public static async Task WaitForActiveTable(this IAmazonDynamoDB client, string userTableName)
        {
            bool active;
            do
            {
                active = true;
                var response = await client.DescribeTableAsync(new DescribeTableRequest {TableName = userTableName});
                if (!Equals(response.Table.TableStatus, TableStatus.ACTIVE) ||
                    !response.Table.GlobalSecondaryIndexes.TrueForAll(g => Equals(g.IndexStatus, IndexStatus.ACTIVE)))
                {
                    active = false;
                }

                Console.WriteLine($"Waiting for table {userTableName} to become active...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            } while (!active);
        }

        public static async Task<IAmazonDynamoDB> CreateTable(
            this string TableName,
            IAmazonDynamoDB client,
            string hashKey = "Id",
            long readCapacityUnits = 5,
            long writeCapacityUnits = 5)
        {
            var request = new CreateTableRequest(
                tableName: TableName,
                keySchema: new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = hashKey,
                        KeyType = KeyType.HASH
                    }
                },
                attributeDefinitions: new List<AttributeDefinition>
                {
                    new AttributeDefinition()
                    {
                        AttributeName = hashKey,
                        AttributeType = ScalarAttributeType.S
                    }
                },
                provisionedThroughput: new ProvisionedThroughput
                {
                    ReadCapacityUnits = readCapacityUnits,
                    WriteCapacityUnits = writeCapacityUnits
                }
            );
            Console.WriteLine("Sending request to build table...");
            try
            {
                var result = await client.CreateTableAsync(request);
                Console.WriteLine("Table created.");
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                Console.WriteLine("Table already exists. Fetching description...");
                var result = await client.DescribeTableAsync(TableName);
                Console.WriteLine($"Using table: {result.Table.TableName} ");
            }

            return client;
        }
    }
}