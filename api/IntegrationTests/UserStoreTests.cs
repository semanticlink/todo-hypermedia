using System.Threading.Tasks;
 using Infrastructure.NoSQL;
 using Xunit;
 
 namespace IntegrationTests
 {
     public class UserStoreTests
     {
         [Fact]
         public async Task LoadUser()
         {
             using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
             {
                 await TableNameConstants
                     .User
                     .CreateTable(dbProvider.Client);
 
                 var userStore = new UserStore(dbProvider.Context);
 
                 var id = await userStore.Create("tenant-id", "idenity-id");
 
                 var user = await userStore.Get(id);
 
                 Assert.Equal("tenant-id", user.tenantId);
 
                 await userStore.Delete(id);
             }
         }
     }
 }