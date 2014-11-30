using Dargon.Services.Networking;
using Dargon.Services.Networking.Server;
using Dargon.Services.Server;
using ItzWarty.Collections;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ServiceNodeTests : NMockitoInstance {
      private readonly ServiceNode testObj;

      [Mock] private readonly ICollectionFactory collectionFactory;
      [Mock] private readonly IConnector connector = null;
      [Mock] private readonly IServiceContextFactory serviceContextFactory = null;
      [Mock] private readonly IDummyService dummyService = null;
      [Mock] private readonly IServiceContext dummyServiceContext = null;

      private readonly ConcurrentDictionary<object, IServiceContext> serviceContextsByService;

      public ServiceNodeTests() {
         serviceContextsByService = new ConcurrentDictionary<object, IServiceContext>();
         When(collectionFactory.CreateConcurrentDictionary<object, IServiceContext>()).ThenReturn(serviceContextsByService);

         testObj = new ServiceNode(collectionFactory, connector, serviceContextFactory);

         Verify(collectionFactory).CreateConcurrentDictionary<object, IServiceContext>();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisteringUnregisteredServiceDelegatesToServiceConnectorTest() {
         When(serviceContextFactory.Create(dummyService)).ThenReturn(dummyServiceContext);

         testObj.RegisterService(dummyService);

         Verify(serviceContextFactory).Create(dummyService);
         Verify(connector, Once()).RegisterService(dummyServiceContext);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisteringRegisteredServiceDoesNothingTest() {
         RegisteringUnregisteredServiceDelegatesToServiceConnectorTest();

         testObj.RegisterService(dummyService);

         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisteringUnregisteredServiceDoesNothingTest() {
         testObj.UnregisterService(dummyService);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisteringRegisteredServiceDelegatesToServiceConnectorTest() {
         RegisteringUnregisteredServiceDelegatesToServiceConnectorTest();

         testObj.UnregisterService(dummyService);
         Verify(connector, Once()).UnregisterService(dummyServiceContext);
         VerifyNoMoreInteractions();
      }

      public interface IDummyService { }
   }
}
