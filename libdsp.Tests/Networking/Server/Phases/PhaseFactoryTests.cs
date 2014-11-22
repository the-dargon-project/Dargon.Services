using Dargon.PortableObjects;
using Dargon.Services.Networking.Server.Sessions;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server.Phases {
   public class PhaseFactoryTests : NMockitoInstance {
      private readonly PhaseFactory testObj;

      [Mock] private readonly IThreadingProxy threadingProxy = null;
      [Mock] private readonly INetworkingProxy networkingProxy = null;
      [Mock] private readonly IHostSessionFactory hostSessionFactory = null;
      [Mock] private readonly IPofSerializer pofSerializer = null;
      [Mock] private readonly IServiceConfiguration serviceConfiguration = null;
      [Mock] private readonly IContext context = null;

      public PhaseFactoryTests() {
         testObj = new PhaseFactory(threadingProxy, networkingProxy, hostSessionFactory, pofSerializer, serviceConfiguration, context);
      }

      [Fact]
      public void CreateIndeterminatePhaseTest() {
         var obj = testObj.CreateIndeterminatePhase();
         VerifyNoMoreInteractions();

         AssertTrue(obj is IndeterminatePhase);
      }

      [Fact]
      public void CreateHostPhaseTest() {
         var hostThread = CreateUntrackedMock<IThread>();
         var cancellationTokenSource = CreateUntrackedMock<ICancellationTokenSource>();
         When(threadingProxy.CreateThread(Any<ThreadEntryPoint>(), Any<ThreadCreationOptions>())).ThenReturn(hostThread);
         When(threadingProxy.CreateCancellationTokenSource()).ThenReturn(cancellationTokenSource);
         var listenerSocket = CreateMock<IListenerSocket>();
         var obj = testObj.CreateHostPhase(listenerSocket);
         Verify(threadingProxy, Once()).CreateThread(Any<ThreadEntryPoint>(), Any<ThreadCreationOptions>());
         Verify(threadingProxy, Once()).CreateCancellationTokenSource();
         VerifyNoMoreInteractions();

         AssertTrue(obj is HostPhase);
      }

      [Fact]
      public void CreateGuestPhaseTest() {
         var clientSocket = CreateMock<IConnectedSocket>();
         var obj = testObj.CreateGuestPhase(clientSocket);
         VerifyNoMoreInteractions();

         AssertTrue(obj is GuestPhase);
      }
   }
}
