using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.Utilities;
using NMockito;
using System.Runtime.InteropServices;
using Xunit;

namespace Dargon.Services {
   public class ServiceClientProxyImplTests : NMockitoInstance {
      private const string kTestInterfaceGuidString = "77EBB5DF-FD4B-47FA-8488-A50A2B0EFA29";
      private readonly Guid kTestInterfaceGuid = Guid.Parse(kTestInterfaceGuidString);
      private readonly object kDummyServiceImplementation = new object();
      private readonly Type kDummyServiceType = typeof(TestInterface);

      [Mock(StaticType = typeof(AttributeUtilities))] private readonly AttributeUtilitiesInterface attributeUtilities = null;
      [Mock] private readonly LocalServiceRegistry localServiceRegistry = null;
      [Mock] private readonly RemoteServiceProxyContainer remoteServiceProxyContainer = null;
      private ServiceClientProxyImpl testObj;

      public ServiceClientProxyImplTests() {
         testObj = new ServiceClientProxyImpl(localServiceRegistry, remoteServiceProxyContainer);
      }

      [Fact]
      public void RegisterService_WithoutGuid_HappyPathTest() {
         var guidPlaceholder = CreatePlaceholder<Guid>();
         When(attributeUtilities.TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder)).Set(guidPlaceholder, kTestInterfaceGuid).ThenReturn(true);

         testObj.RegisterService(kDummyServiceImplementation, kDummyServiceType);

         Verify(attributeUtilities).TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder);
         Verify(localServiceRegistry).RegisterService(kDummyServiceImplementation, kDummyServiceType, kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterService_WithoutGuidAndAttribute_ThrowsExceptionTest() {
         var guidPlaceholder = CreatePlaceholder<Guid>();
         When(attributeUtilities.TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder)).ThenReturn(false);

         AssertThrows<ArgumentException>(() => testObj.RegisterService(kDummyServiceImplementation, kDummyServiceType));

         Verify(attributeUtilities).TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterService_WithGuid_DelegatesToLocalServiceRegistryTest() {
         testObj.RegisterService(kDummyServiceImplementation, kDummyServiceType, kTestInterfaceGuid);
         Verify(localServiceRegistry).RegisterService(kDummyServiceImplementation, kDummyServiceType, kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisterService_WithoutGuid_HappyPathTest() {
         var guidPlaceholder = CreatePlaceholder<Guid>();
         When(attributeUtilities.TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder)).Set(guidPlaceholder, kTestInterfaceGuid).ThenReturn(true);

         testObj.UnregisterService(kDummyServiceImplementation, kDummyServiceType);

         Verify(attributeUtilities).TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder);
         Verify(localServiceRegistry).UnregisterService(kDummyServiceImplementation, kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisterService_WithoutGuidAndAttribute_ThrowsExceptionTest() {
         var guidPlaceholder = CreatePlaceholder<Guid>();
         When(attributeUtilities.TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder)).ThenReturn(false);

         AssertThrows<ArgumentException>(() => testObj.UnregisterService(kDummyServiceImplementation, kDummyServiceType));

         Verify(attributeUtilities).TryGetInterfaceGuid(kDummyServiceType, out guidPlaceholder);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisterService_WithGuid_DelegatesToLocalServiceRegistryTest() {
         testObj.UnregisterService(kDummyServiceImplementation, kTestInterfaceGuid);
         Verify(localServiceRegistry).UnregisterService(kDummyServiceImplementation, kTestInterfaceGuid);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void GetService_WithInterface_DelegatesToRemoteServiceProxyContainerTest() {
         var remoteService = CreateMock<TestInterface>();
         When(remoteServiceProxyContainer.GetService<TestInterface>()).ThenReturn(remoteService);
         AssertEquals(remoteService, testObj.GetService<TestInterface>());
         Verify(remoteServiceProxyContainer).GetService<TestInterface>();
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

      /// <summary>
      /// No service guid specified here for purpose of test.
      /// </summary>
      public interface TestInterface { }
   }
}
