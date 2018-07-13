using System;
using NLog;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public abstract class BaseTestProvider : IDisposable
    {
        protected readonly DynamoDbServerTestUtils.DisposableDatabase DbProvider;
        protected static ILogger Log;
        

        protected BaseTestProvider()
        {
            DbProvider = DynamoDbServerTestUtils.CreateDatabase();
        }

        protected BaseTestProvider(ITestOutputHelper output) : this()

        {
            // need to be able to see the output in the console window
            Log = output.GetNLogLogger();
            
            // TODO: remember if we need to write assertions on log messages use Divergic.Logging.Xunit
        }

        public void Dispose()
        {
            DbProvider.Dispose();
            Log.RemoveTestOutputHelper();
        }
    }
}