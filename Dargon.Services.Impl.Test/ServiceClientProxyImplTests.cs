using System;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ServiceClientProxyImplTests : NMockitoInstance {
      private const string kTestInterfaceGuidString = "77EBB5DF-FD4B-47FA-8488-A50A2B0EFA29";
      private readonly Guid kTestInterfaceGuid = Guid.Parse(kTestInterfaceGuidString);
      private readonly object kDummyServiceImplementation = new object();
      private readonly Type kDummyServiceType = typeof(TestInterface);

      [Mock] private readonly LocalServiceRegistry localServiceRegistry = null;
      [Mock] private readonly RemoteServiceProxyContainer remoteServiceProxyContainer = null;
      private ServiceClientProxyImpl testObj;

      public ServiceClientProxyImplTests() {
         testObj = new ServiceClientProxyImpl(localServiceRegistry, remoteServiceProxyContainer);
      }
      
      [Fact]
      public void RegisterService_WithGuid_DelegatesToLocalServiceRegistryTest() {
         testObj.RegisterService(kDummyServiceImplementation, kDummyServiceType, kTestInterfaceGuid);
         Verify(localServiceRegistry).RegisterService(kDummyServiceImplementation, kDummyServiceType, kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }
      
      [Fact]
      public void UnregisterService_WithGuid_DelegatesToLocalServiceRegistryTest() {
         testObj.UnregisterService(kTestInterfaceGuid);
         Verify(localServiceRegistry).UnregisterService(kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void GetService_WithInterfaceAndServiceGuid_DelegatesToRemoteServiceProxyContainerTest() {
         var serviceGuid = CreatePlaceholder<Guid>();
         var remoteService = CreateMock<TestInterface>();
         When(remoteServiceProxyContainer.GetService<TestInterface>(serviceGuid)).ThenReturn(remoteService);
         AssertEquals(remoteService, testObj.GetService<TestInterface>(serviceGuid));
         Verify(remoteServiceProxyContainer).GetService<TestInterface>(serviceGuid);
         VerifyNoMoreInteractions();
      }

      public interface TestInterface { }
   }
}
