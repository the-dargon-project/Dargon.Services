using Dargon.Ryu;
using NMockito;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Dargon.Services {
   public class OutTests : NMockitoInstance {
      private static readonly Guid kWartyAccountId = Guid.NewGuid();
      private static readonly Guid kFredAccountId = Guid.NewGuid();
      private const int kTestServicePort = 30001;

      [Fact]
      public void Run() {
         var ryu = new RyuFactory().Create();
         ryu.Setup();
         var serviceClientFactory = ryu.Get<IServiceClientFactory>();
         var serverClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.HostOnly);
         var serverServiceClient = serviceClientFactory.CreateOrJoin(serverClusteringConfiguration);
         serverServiceClient.RegisterService(new ExampleImplementation(), typeof(ExampleInterface));

         var clientClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.GuestOnly);
         var clientServiceClient = serviceClientFactory.CreateOrJoin(clientClusteringConfiguration);

         var remoteService = clientServiceClient.GetService<ExampleInterface>();

         var sw = new Stopwatch();
         for (var i = 0; i < 10; i++) {
            sw.Restart();

            Guid accountId;
            AssertTrue(remoteService.TryAuthenticate("warty", "asdf", out accountId));
            AssertEquals(kWartyAccountId, accountId);

            AssertTrue(remoteService.TryAuthenticate("fred", "qwerty", out accountId));
            AssertEquals(kFredAccountId, accountId);

            AssertFalse(remoteService.TryAuthenticate("larry", "zxcv", out accountId));
            AssertEquals(Guid.Empty, accountId);

            Debug.WriteLine("Three queries performed in: " + sw.ElapsedMilliseconds);
         }
      }

      [Guid("2257424F-C403-4CA2-A670-8F1FD896F2FB")]
      public interface ExampleInterface {
         bool TryAuthenticate(string username, string password, out Guid accountId);
      }

      public class ExampleImplementation : ExampleInterface {
         public bool TryAuthenticate(string username, string password, out Guid accountId) {
            if (username == "warty" && password == "asdf") {
               accountId = kWartyAccountId;
               return true;
            } else if (username == "fred" && password == "qwerty") {
               accountId = kFredAccountId;
               return true;
            } else {
               accountId = Guid.Empty;
               return false;
            }
         }
      }
   }
}
