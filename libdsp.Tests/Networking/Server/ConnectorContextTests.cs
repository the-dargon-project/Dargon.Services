using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server {
   public class ConnectorContextTests : NMockitoInstance {
      private readonly ConnectorContext testObj;
      [Mock] private readonly IPhase initialPhase = null;
      [Mock] private readonly IPhase otherPhase = null;
      [Mock] private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName = null;

      public ConnectorContextTests() {
         testObj = new ConnectorContext(initialPhase);
      }

      [Fact]
      public void InitializeHappyPathTest() {
         testObj.Initialize(serviceContextsByName);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ServiceContextsByNameReflectsInitializeParameterTest() {
         InitializeHappyPathTest();

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
      public void HandleUpdateDelegatesToCurrentPhaseTest() {
         testObj.HandleUpdate();

         Verify(initialPhase).HandleUpdate();
         VerifyNoMoreInteractions();
      }
   }
}
