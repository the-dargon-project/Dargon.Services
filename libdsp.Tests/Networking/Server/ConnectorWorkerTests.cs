using System.Threading;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server {
   public class ConnectorWorkerTests : NMockitoInstance {
      private readonly ConnectorWorker testObj;

      [Mock] private readonly IThreadingProxy threadingProxy = null;
      [Mock] private readonly IContext context = null;
      [Mock] private readonly ICancellationTokenSource cancellationTokenSource = null;
      [Mock] private readonly ISemaphore updateSemaphore = null;
      [Mock] private readonly IThread workerThread = null;
      [Mock] private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName = null;
      [Mock] private readonly ICancellationToken cancellationToken = null;

      public ConnectorWorkerTests() {
         When(threadingProxy.CreateCancellationTokenSource()).ThenReturn(cancellationTokenSource);
         When(threadingProxy.CreateSemaphore(0, int.MaxValue)).ThenReturn(updateSemaphore);
         When(threadingProxy.CreateThread(Any<ThreadEntryPoint>(), Any<ThreadCreationOptions>())).ThenReturn(workerThread);

         this.testObj = new ConnectorWorker(threadingProxy, context);
         
         var threadCreationOptionsCaptor = new ArgumentCaptor<ThreadCreationOptions>();
         Verify(threadingProxy, Once()).CreateCancellationTokenSource();
         Verify(threadingProxy, Once()).CreateSemaphore(0, int.MaxValue);
         Verify(threadingProxy, Once()).CreateThread(Eq<ThreadEntryPoint>(testObj.ThreadEntryPoint), threadCreationOptionsCaptor.GetParameter());
         VerifyNoMoreInteractions();

         AssertTrue(threadCreationOptionsCaptor.Value.IsBackground);

         When(cancellationTokenSource.Token).ThenReturn(cancellationToken);
      }

      [Fact]
      public void InitializeHappyPathTest() {
         testObj.Initalize(serviceContextsByName);

         Verify(context).Initialize(serviceContextsByName);
         Verify(workerThread).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void StartHappyPathTest() {
         testObj.Start();

         Verify(updateSemaphore, Once()).Release();
         Verify(workerThread, Once(), AfterPrevious()).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SignalUpdateReleasesSemaphoreCountTest() {
         testObj.SignalUpdate();

         Verify(updateSemaphore, Once()).Release();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DisposeHappyPathTest() {
         testObj.Dispose();

         Verify(cancellationTokenSource).Cancel();
         Verify(updateSemaphore).Release();
         Verify(workerThread).Join();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ThreadEntryPointImmediatelyCancelledTest() {
         When(cancellationTokenSource.IsCancellationRequested).ThenReturn(true);

         testObj.ThreadEntryPoint();

         Verify(cancellationTokenSource).IsCancellationRequested.Wrap();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ThreadEntryPointCancelledBeforeUpdateTest() {
         When(cancellationTokenSource.IsCancellationRequested).ThenReturn(false, true);
         When(updateSemaphore.Wait(cancellationToken)).ThenReturn(false);

         testObj.ThreadEntryPoint();

         Verify(cancellationTokenSource).IsCancellationRequested.Wrap();
         Verify(cancellationTokenSource).Token.Wrap();
         Verify(updateSemaphore, Once()).Wait(cancellationToken);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ThreadEntryPointHappyPathTest() {
         When(cancellationTokenSource.IsCancellationRequested).ThenReturn(false, false, true);
         When(updateSemaphore.Wait(cancellationToken)).ThenReturn(true);

         testObj.ThreadEntryPoint();

         Verify(cancellationTokenSource).IsCancellationRequested.Wrap();
         Verify(cancellationTokenSource).Token.Wrap();
         Verify(updateSemaphore, Times(2)).Wait(cancellationToken);
         Verify(context, Times(2)).HandleUpdate();
         VerifyNoMoreInteractions();
      }
   }
}
