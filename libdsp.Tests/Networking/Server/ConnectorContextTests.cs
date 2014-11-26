using System;
using System.Diagnostics;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server {
   public class ConnectorContextTests : NMockitoInstance {
      private readonly ConnectorConnectorContext testObj;
      [Mock] private readonly IPhase initialPhase = null;
      [Mock] private readonly IPhase otherPhase = null;
      [Mock] private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName = null;

      public ConnectorContextTests() {
         testObj = new ConnectorConnectorContext(initialPhase);
      }

      [Fact]
      public void InitializeHappyPathTest() {
         testObj.Initialize(serviceContextsByName);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ServiceContextsByNameReflectsInitializeParameterTest() {
         InitializeHappyPathTest();

         Debug.WriteLine(serviceContextsByName);
         Debug.WriteLine(testObj.ServiceContextsByName);
         AssertEquals(serviceContextsByName, testObj.ServiceContextsByName);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void CurrentPhaseInitiallyReflectsConstructorParameterTest() {
         AssertEquals(initialPhase, testObj.CurrentPhase);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void TransitionSetsCurrentPhaseTest() {
         testObj.Transition(otherPhase);

         VerifyNoMoreInteractions();

         AssertEquals(otherPhase, testObj.CurrentPhase);
      }

      [Fact]
      public void TransitionThrowsIfDisposedTest() {
         testObj.Dispose();
         ClearInteractions();

         Assert.Throws<ObjectDisposedException>(() => testObj.Transition(otherPhase));
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RunIterationDelegatesToCurrentPhaseTest() {
         testObj.RunIteration();

         Verify(initialPhase).RunIteration();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RunIterationThrowsIfDisposedTest() {
         testObj.Dispose();
         ClearInteractions();

         Assert.Throws<ObjectDisposedException>(() => testObj.RunIteration());
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DisposeTest() {
         testObj.Dispose();
         Assert.Throws<ObjectDisposedException>(() => testObj.ThrowIfDisposed());
         Verify(initialPhase).Dispose();
         VerifyNoMoreInteractions();
      }
   }
}
