using System;
using System.Collections.Generic;
using Dargon.Services.Networking;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services.Tests {
   public class ServiceConnectorTests : NMockitoInstance {
      private ServiceConnector testObj;

      [Mock] private readonly IServiceConnectorWorkerFactory serviceConnectorWorkerFactory = null;
      [Mock] private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName = null;
      [Mock] private readonly IServiceContext serviceContext = null;
      [Mock] private readonly IServiceConnectorWorker serviceConnectorWorker = null;

      private const string kServiceName = "Service Name!";

      public ServiceConnectorTests() {
         testObj = new ServiceConnector(serviceConnectorWorkerFactory, serviceContextsByName);

         When(serviceContext.Name).ThenReturn(kServiceName);
      }

      [Fact]
      public void InitializeSpawnsServiceConnectorWorkerTest() {
         When(serviceConnectorWorkerFactory.Create(serviceContextsByName)).ThenReturn(serviceConnectorWorker);
         
         testObj.Initialize();

         Verify(serviceConnectorWorkerFactory).Create(serviceContextsByName);
         Verify(serviceConnectorWorker).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DuplicateRegisterServiceThrowsExceptionTest() {
         InitializeSpawnsServiceConnectorWorkerTest();

         When(serviceContextsByName.TryAdd(kServiceName, serviceContext)).ThenReturn(false);
         Assert.Throws<InvalidOperationException>(() => testObj.RegisterService(serviceContext));
         Verify(serviceContext).Name.Wrap();
         Verify(serviceContextsByName).TryAdd(kServiceName, serviceContext);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterServiceHappyPathTest() {
         InitializeSpawnsServiceConnectorWorkerTest();

         When(serviceContextsByName.TryAdd(kServiceName, serviceContext)).ThenReturn(true);
         testObj.RegisterService(serviceContext);
         Verify(serviceContext).Name.Wrap();
         Verify(serviceContextsByName).TryAdd(kServiceName, serviceContext);
         Verify(serviceConnectorWorker).SignalUpdate();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DeregisterUnregisteredServiceThrowsExceptionTest() {
         IServiceContext dummy;
         When(serviceContextsByName.TryRemove(kServiceName, out dummy)).ThenReturn(false);
         Assert.Throws<InvalidOperationException>(() => testObj.UnregisterService(serviceContext));
         Verify(serviceContext).Name.Wrap();
         Verify(serviceContextsByName).TryRemove(kServiceName, out dummy);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DeregisterServiceHappyPathTest() {
         IServiceContext dummy = Any<IServiceContext>();
         When(serviceContextsByName.TryRemove(kServiceName, out dummy)).Set(dummy, true).ThenReturn(true);
         testObj.UnregisterService(serviceContext);
         Verify(serviceContext).Name.Wrap();
         Verify(serviceContextsByName).TryRemove(kServiceName, out dummy);
         VerifyNoMoreInteractions();
      }
   }
}
