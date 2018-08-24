using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public static class DynamoUtils
    {

        public static async Task WaitForAllTables(this IAmazonDynamoDB client, ILogger log)
        {
            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.WaitForActiveTable(client, log)));
        }

        public static async Task CreateAllTables(this IAmazonDynamoDB client, ILogger log)
        {
            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.CreateTable(client, log)));
        }

        public static async Task WaitForActiveTable(this string userTableName, IAmazonDynamoDB client, ILogger log)
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
                catch (ResourceNotFoundException)
                {
                    log.Debug("Waiting for Dynamo to be available");
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

                log.Debug($"Waiting for table {userTableName} to become active...");
                await Task.Delay(TimeSpan.FromSeconds(1));
            } while (!active);

            log.Debug($"Table {userTableName} active");
        }

        public static async Task<IAmazonDynamoDB> CreateTable(
            this string tableName,
            IAmazonDynamoDB client,
            ILogger log,
            string hashKey = "Id",
            long readCapacityUnits = 5,
            long writeCapacityUnits = 5)
        {
            var request = new CreateTableRequest(
                tableName: tableName,
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
            log.Debug($"Building table: {tableName}");
            try
            {
                await client.CreateTableAsync(request);
                log.Debug($"Table created: {tableName}");
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                log.Debug($"Table already exists: {tableName}");
                var result = await client.DescribeTableAsync(tableName);
                log.Debug($"Using: {result.Table.TableName} ");
            }

            return client;
        }
    }
}