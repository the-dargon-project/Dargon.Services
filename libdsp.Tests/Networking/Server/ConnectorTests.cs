using System;
using ItzWarty.Collections;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server {
   public class ConnectorTests : NMockitoInstance {
      private readonly Connector testObj;

      [Mock] private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName = null;
      [Mock(Tracking.Untracked)] private readonly IServiceContext serviceContext = null;
      [Mock] private readonly IConnectorWorker connectorWorker = null;

      private const string kServiceName = "Service Name!";

      public ConnectorTests() {
         testObj = new Connector(connectorWorker, serviceContextsByName);

         When(serviceContext.Name).ThenReturn(kServiceName);
      }

      [Fact]
      public void InitializeHappyPathTest() {       
         testObj.Initialize();

         Verify(connectorWorker, Once()).Initalize(serviceContextsByName);
         Verify(connectorWorker, Once(), AfterPrevious()).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DuplicateRegisterServiceThrowsExceptionTest() {
         When(serviceContextsByName.TryAdd(kServiceName, serviceContext)).ThenReturn(false);
         Assert.Throws<InvalidOperationException>(() => testObj.RegisterService(serviceContext));
         Verify(serviceContextsByName).TryAdd(kServiceName, serviceContext);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterServiceHappyPathTest() {
         When(serviceContextsByName.TryAdd(kServiceName, serviceContext)).ThenReturn(true);
         testObj.RegisterService(serviceContext);
         Verify(serviceContextsByName).TryAdd(kServiceName, serviceContext);
         Verify(connectorWorker).SignalUpdate();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DeregisterUnregisteredServiceThrowsExceptionTest() {
         IServiceContext dummy;
         When(serviceContextsByName.TryRemove(kServiceName, out dummy)).ThenReturn(false);
         Assert.Throws<InvalidOperationException>(() => testObj.UnregisterService(serviceContext));
         Verify(serviceContextsByName).TryRemove(kServiceName, out dummy);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DeregisterIncorrectServiceThrowsExceptionTest() {
         var serviceContextRef = CreateRef<IServiceContext>();
         var otherServiceContext = CreateUntrackedMock<IServiceContext>();
         When(serviceContextsByName.TryRemove(kServiceName, out serviceContextRef)).Set(serviceContextRef, otherServiceContext).ThenReturn(true);
         Assert.Throws<InvalidOperationException>(() => testObj.UnregisterService(serviceContext));
         Verify(serviceContextsByName).TryRemove(kServiceName, out serviceContextRef);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DeregisterServiceHappyPathTest() {
         var serviceContextRef = CreateRef<IServiceContext>();
         When(serviceContextsByName.TryRemove(kServiceName, out serviceContextRef)).Set(serviceContextRef, serviceContext).ThenReturn(true);
         testObj.UnregisterService(serviceContext);
         Verify(serviceContextsByName).TryRemove(kServiceName, out serviceContextRef);
         Verify(connectorWorker).SignalUpdate();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DisposeHappyPathTest() {
         testObj.Dispose();

         Verify(connectorWorker).Dispose();
         VerifyNoMoreInteractions();
      }
   }
}
