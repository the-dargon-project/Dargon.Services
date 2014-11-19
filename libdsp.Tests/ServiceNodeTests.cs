using Dargon.Services.Networking;
using NMockito;
using Xunit;

namespace Dargon.Services.Tests {
   public class ServiceNodeTests : NMockitoInstance {
      private ServiceNode testObj;

      [Mock] private readonly IServiceConnector serviceConnector = null;
      [Mock] private readonly IServiceContextFactory serviceContextFactory = null;
      [Mock] private readonly IDummyService dummyService = null;
      [Mock] private readonly IServiceContext dummyServiceContext = null;

      public ServiceNodeTests() {
         testObj = new ServiceNode(serviceConnector, serviceContextFactory);
      }

      [Fact]
      public void RegisteringUnregisteredServiceDelegatesToServiceConnectorTest() {
         When(serviceContextFactory.Create(dummyService)).ThenReturn(dummyServiceContext);

         testObj.RegisterService(dummyService);

         Verify(serviceContextFactory).Create(dummyService);
         Verify(serviceConnector, Once()).RegisterService(dummyServiceContext);
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
         Verify(serviceConnector, Once()).UnregisterService(dummyServiceContext);
         VerifyNoMoreInteractions();
      }

      public interface IDummyService { }
   }
}
