using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App;
using Domain;
using Domain.Models;
using Infrastructure.NoSQL;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public abstract class BaseTestProvider : IDisposable
    {
        protected static ILogger Log;
        protected ServiceProvider ServiceProvider;
        private readonly ServiceCollection _services;


        protected BaseTestProvider()
        {
            // TODO: use Fixtures

            _services = new ServiceCollection();
            // Register up the repositories to make them available
            Register(iocRegistrations =>
            {
                iocRegistrations
                    .RegisterInfrastructure( /* isDevelopment) */true)
                    .RegisterRespositories();
            });
            
            UserId = RegisterUser();

            Startup();
        }

        protected string UserId { get; private set; }

        protected BaseTestProvider(ITestOutputHelper output) : this()
        {
            // need to be able to see the output in the console window
            Log = output.GetNLogLogger();

            // TODO: remember if we need to write assertions on log messages use Divergic.Logging.Xunit
        }

        private void Startup()
        {
            var client = Get<IAmazonDynamoDB>();

            TableNameConstants
                .AllTables
                .ForEach(table => table.CreateTable(client).ConfigureAwait(false));

            Task.Run(() => Task.WhenAll(
                    TableNameConstants
                        .AllTables
                        .Select(table => table.WaitForActiveTable(client))))
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        ///     Resolves the aleady registered service required for the test
        /// </summary>
        protected T Get<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        protected void Register(Action<ServiceCollection> iocRegistration)
        {
            iocRegistration(_services);
            ServiceProvider = _services.BuildServiceProvider();
        }

        /// <summary>
        ///     Register a new user to be injected and get the id for use in the tests.
        /// </summary>
        /// <remarks>
        ///    This emulates the http context of an authenticated user.
        /// </remarks>
        private string RegisterUser()
        {
            var userId = NewId();
            Register(services => { services.AddTransient(ctx => new User {Id = userId}); });
            return userId;
        }

        protected string NewId()
        {
            return ServiceProvider.GetService<IIdGenerator>().New();
        }

        protected IDynamoDBContext Db => ServiceProvider.GetService<IDynamoDBContext>();


        public void Dispose()
        {
            ServiceProvider.Dispose();
            try
            {
                Log.RemoveTestOutputHelper();
            }
            catch (Exception e)
            {
                Log.Error(e, "Ensure that the test calls ':base(outputHelper)'");
                throw;
            }
        }
    }
}