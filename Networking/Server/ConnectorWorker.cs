using ItzWarty.Collections;
using ItzWarty.Threading;

namespace Dargon.Services.Networking.Server {
   class ConnectorWorker : IConnectorWorker {
      private readonly IThreadingProxy threadingProxy;
      private readonly IContext context;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly ISemaphore updateSemaphore;
      private readonly IThread workerThread;

      public ConnectorWorker(IThreadingProxy threadingProxy, IContext context) {
         this.threadingProxy = threadingProxy;
         this.context = context;
         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.updateSemaphore = threadingProxy.CreateSemaphore(0, int.MaxValue);
         this.workerThread = threadingProxy.CreateThread(ThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
      }

      public void Initalize(IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.context.Initialize(serviceContextsByName);
         this.workerThread.Start();
      }

      internal void ThreadEntryPoint() {
         while (!this.cancellationTokenSource.IsCancellationRequested) {
            if (this.updateSemaphore.Wait(cancellationTokenSource.Token)) {
               this.context.HandleUpdate();
            }
         }
      }

      public void Start() {
         this.updateSemaphore.Release();
         this.workerThread.Start();
      }

      public void SignalUpdate() {
         this.updateSemaphore.Release();
      }

      public void Dispose() {
         this.cancellationTokenSource.Cancel();
         this.updateSemaphore.Release();
         this.workerThread.Join();
      }
   }
}