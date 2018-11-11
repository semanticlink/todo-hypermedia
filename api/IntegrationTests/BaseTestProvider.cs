using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Api;
using Api.Web;
using Domain;
using Domain.Models;
using Infrastructure.NoSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public abstract class BaseTestProvider : IDisposable
    {
        protected ILogger Log { get; }
        protected IServiceProvider ServiceProvider;
        private readonly ServiceCollection _services;


        private BaseTestProvider()
        {
            // TODO: use Fixtures

            _services = new ServiceCollection();
            
            // Register up the repositories to make them available
            Register(iocRegistrations =>
            {
                iocRegistrations
                    .RegisterInfrastructure(isDevelopment: true)
                    .RegisterRepositories()
                    // various discussion here on unit test with the ILogger in xunit
                    // see https://stackoverflow.com/questions/43424095/how-to-unit-test-with-ilogger-in-asp-net-core
                    // at this stage, going to log out nothing to xunit console by registering a null logger factory
                    // and a logger 
                    .AddScoped<ILoggerFactory, NullLoggerFactory>()
                    .AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            });

            UserId = RegisterUser();
            Log = ServiceProvider.GetService<ILoggerFactory>().CreateLogger<BaseTestProvider>();

            Startup();
            
        }

        protected string UserId { get; private set; }

        protected BaseTestProvider(ITestOutputHelper output) : this()
        {
            // some methods require the Logger to be passed in
//            Log = ServiceProvider.GetService<ILogger>();
            Log = ServiceProvider.GetService<ILoggerFactory>().CreateLogger<BaseTestProvider>();

            // TODO: remember if we need to write assertions on log messages use Divergic.Logging.Xunit
        }

        /// <summary>
        ///     At startup, ensure that all the tables are created and active
        /// </summary>
        private void Startup()
        {
            var client = Get<IAmazonDynamoDB>();

            TableNameConstants
                .AllTables
                .ForEach(table => table.CreateTable(client, Log).ConfigureAwait(false));

            Task.Run(() => Task.WhenAll(
                    TableNameConstants
                        .AllTables
                        .Select(table => table.WaitForActiveTable(client, Log))))
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
            if (ServiceProvider as ServiceProvider is ServiceProvider provider)
            {
                provider.Dispose();
            }
        }
    }
}