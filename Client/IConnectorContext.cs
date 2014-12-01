using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public interface IConnectorContext : IDisposable {
      ICancellationToken CancellationToken { get; }
   }

   public interface IUserConnectorContext : IConnectorContext {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }

   public interface IInvocationRequestConsumerContext : IConnectorContext {
      bool TryTakeUnsentInvocation(out IInvocationState invocationState);
   }

   public interface IInvocationResponseProducerContext : IConnectorContext {
      void HandleInvocationResult(uint invocationId, object result);
   }

   public class ConnectorContext : IUserConnectorContext, IInvocationRequestConsumerContext, IInvocationResponseProducerContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly IInvocationStateFactory invocationStateFactory;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IConcurrentQueue<IInvocationState> unsentRequestQueue;
      private readonly ISemaphore unsentRequestSemaphore;
      private readonly IConcurrentDictionary<uint, IInvocationState> pendingInvocationsById;
      
      // reader/writer references held so they are disposed with context.
      private IMessageReader reader;
      private IMessageWriter writer;

      public ConnectorContext(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, IInvocationStateFactory invocationStateFactory) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.invocationStateFactory = invocationStateFactory;

         this.availableInvocationIds = collectionFactory.CreateUniqueIdentificationSet(true);
         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.unsentRequestQueue = collectionFactory.CreateConcurrentQueue<IInvocationState>();
         this.unsentRequestSemaphore = threadingProxy.CreateSemaphore(0, int.MaxValue);
         this.pendingInvocationsById = collectionFactory.CreateConcurrentDictionary<uint, IInvocationState>();
      }

      public ICancellationToken CancellationToken { get { return cancellationTokenSource.Token; } }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         var invocationId = availableInvocationIds.TakeUniqueID();
         var invocationState = invocationStateFactory.Create(invocationId, serviceGuid, methodName, methodArguments);
         pendingInvocationsById.Add(invocationId, invocationState);
         unsentRequestQueue.Enqueue(invocationState);
         unsentRequestSemaphore.Release();
         invocationState.Wait();
         return invocationState.Result;
      }

      bool IInvocationRequestConsumerContext.TryTakeUnsentInvocation(out IInvocationState invocationState) {
         invocationState = null;
         if (unsentRequestSemaphore.Wait(cancellationTokenSource.Token)) {
            var spinner = new SpinWait();
            while (!unsentRequestQueue.TryDequeue(out invocationState)) {
               spinner.SpinOnce();
            }
         }
         return invocationState != null;
      }

      void IInvocationResponseProducerContext.HandleInvocationResult(uint invocationId, object result) {
         IInvocationState invocationState;
         if (!pendingInvocationsById.TryRemove(invocationId, out invocationState)) {
            throw new InvalidOperationException("Received invocation result for unknown invocation id " + invocationId);
         }
         invocationState.SetResult(result);
         availableInvocationIds.GiveUniqueID(invocationId);
      }

      public void SetReader(IMessageReader reader) {
         this.reader = reader;
      }

      public void SetWriter(IMessageWriter writer) {
         this.writer = writer;
      }

      public void Dispose() {
         this.cancellationTokenSource.Cancel();
         this.reader.Dispose();
         this.writer.Dispose();
      }
   }
}