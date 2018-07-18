using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain.Models;
using NLog;

namespace Infrastructure.NoSQL
{
    public static class DynamoUtils
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();


        public static async Task WaitForAllTables(this IAmazonDynamoDB client)
        {
            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.WaitForActiveTable(client)));
        }

        public static async Task CreateAllTables(this IAmazonDynamoDB client)
        {
            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.CreateTable(client)));
        }

        public static async Task WaitForActiveTable(this string userTableName, IAmazonDynamoDB client)
        {
            int count = 0;
            bool active;
            do
            {
                active = true;
                DescribeTableResponse response;
                try
                {
                    response = await client.DescribeTableAsync(new DescribeTableRequest {TableName = userTableName});
                }
                catch (ResourceNotFoundException e)
                {
                    Console.WriteLine("Waiting for Dynamo to be available");
                    count++;
                    if (count > 5)
                    {
                        throw;
                    }

                    return;
                }

                if (!Equals(response.Table.TableStatus, TableStatus.ACTIVE) ||
                    !response.Table.GlobalSecondaryIndexes.TrueForAll(g => Equals(g.IndexStatus, IndexStatus.ACTIVE)))
                {
                    active = false;
                }

                Log.Debug($"Waiting for table {userTableName} to become active...");
                Console.WriteLine($"Waiting for table {userTableName} to become active...");
                await Task.Delay(TimeSpan.FromSeconds(1));
            } while (!active);

            Log.Debug($"Table {userTableName} active");
            Console.WriteLine($"Table {userTableName} active");
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
            Log.Debug($"Building table: {TableName}");
            try
            {
                var result = await client.CreateTableAsync(request);
                Log.Debug($"Table created: {TableName}");
                Console.WriteLine($"Table created: {TableName}");
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                Log.Debug($"Table already exists: {TableName}");
                var result = await client.DescribeTableAsync(TableName);
                Log.Debug($"Using: {result.Table.TableName} ");
                Console.WriteLine($"Using: {result.Table.TableName} ");
            }

            return client;
        }
    }
}