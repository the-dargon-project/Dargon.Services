using ItzWarty.Threading;

namespace Dargon.Services.Server {
   public class ConnectorWorker : IConnectorWorker {
      private readonly IThreadingProxy threadingProxy;
      private readonly IConnectorContext connectorContext;
      private readonly IServiceConfiguration serviceConfiguration;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly ISemaphore updateSemaphore;
      private readonly IThread workerThread;

      public ConnectorWorker(IThreadingProxy threadingProxy, IConnectorContext connectorContext, IServiceConfiguration serviceConfiguration) {
         this.threadingProxy = threadingProxy;
         this.connectorContext = connectorContext;
         this.serviceConfiguration = serviceConfiguration;
         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.updateSemaphore = threadingProxy.CreateSemaphore(0, int.MaxValue);
         this.workerThread = threadingProxy.CreateThread(ThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
      }

      public void Initalize() {
         this.workerThread.Start();
      }

      internal void ThreadEntryPoint() {
         while (!this.cancellationTokenSource.IsCancellationRequested) {
            this.connectorContext.RunIteration();

            this.cancellationTokenSource.Token.WaitForCancellation(serviceConfiguration.HeartbeatIntervalMilliseconds);
         }
      }

      public void Dispose() {
         this.cancellationTokenSource.Cancel();
         this.updateSemaphore.Release();
         this.workerThread.Join();
      }
   }
}