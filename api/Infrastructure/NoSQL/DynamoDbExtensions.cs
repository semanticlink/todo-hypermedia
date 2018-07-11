﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public static class DynamoDbExtensions
    {
        public static async Task<IEnumerable<T>> Where<T>(
            this IDynamoDBContext context,
            List<ScanCondition> scanCondition)
            where T : class
        {
            var enumerable = await context
                .ScanAsync<T>(scanCondition.IsNull() ? new List<ScanCondition>() : scanCondition)
                .GetRemainingAsync();
            
            return enumerable;
        }

        public static async Task<IEnumerable<T>> Where<T>(
            this IDynamoDBContext context,
            ScanCondition scanCondition = null)
            where T : class
        {
            return await context.Where<T>(!scanCondition.IsNull()
                ? new List<ScanCondition> {scanCondition}
                : null);
        }

        public static async Task<IEnumerable<T>> Where<T>(this IDynamoDBContext context, string key, string id)
            where T : class
        {
            return await context.Where<T>(new ScanCondition(key, ScanOperator.Equal, id));
        }

        public static async Task<IEnumerable<T>> Where<T>(this IDynamoDBContext context, string id)
            where T : class
        {
            return await context.Where<T>(new ScanCondition(HashKeyConstants.Default, ScanOperator.Equal, id));
        }

        public static async Task<T> WhereById<T>(this IDynamoDBContext context, string id)
            where T : class
        {
            return await context.FirstOrDefault<T>(HashKeyConstants.Default, id);
        }

        public static async Task<IEnumerable<T>> WhereByIds<T>(this IDynamoDBContext context, List<string> ids = null)
            where T : class
        {
            return ids.IsNull()
                ? new List<T>()
                : await context.Where<T>(new ScanCondition(HashKeyConstants.Default, ScanOperator.In, ids.Distinct().ToArray()));
        }

        public static async Task<T> SingleOrDefault<T>(this IDynamoDBContext context, List<ScanCondition> scanConditions)
            where T : class
        {
            return (await context.Where<T>(scanConditions)).SingleOrDefault();
        }

        public static async Task<T> SingleOrDefault<T>(this IDynamoDBContext context, ScanCondition scanCondition)
            where T : class
        {
            return (await context.Where<T>(scanCondition)).SingleOrDefault();
        }

        public static async Task<T> SingleOrDefault<T>(this IDynamoDBContext context, string key, string id)
            where T : class
        {
            return await context.SingleOrDefault<T>(new ScanCondition(key, ScanOperator.Equal, id));
        }

        public static async Task<T> SingleOrDefault<T>(this IDynamoDBContext context, string id)
            where T : class
        {
            return await context.SingleOrDefault<T>(HashKeyConstants.Default, id);
        }

        public static async Task<T> FirstOrDefault<T>(this IDynamoDBContext context, ScanCondition scanCondition)
            where T : class
        {
            return (await context.Where<T>(scanCondition))
                .FirstOrDefault();
        }

        public static async Task<T> FirstOrDefault<T>(this IDynamoDBContext context, string key, string id)
            where T : class
        {
            return await context.FirstOrDefault<T>(new ScanCondition(key, ScanOperator.Equal, id));
        }

        public static async Task<T> FirstOrDefault<T>(this IDynamoDBContext context, string id)
            where T : class
        {
            return await context.FirstOrDefault<T>(HashKeyConstants.Default, id);
        }
    }
}