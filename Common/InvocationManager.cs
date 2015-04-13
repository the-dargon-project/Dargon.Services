using System;
using System.Threading;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using ItzWarty.Collections;
using ItzWarty.Threading;

namespace Dargon.Services.Common {
   public interface InvocationManager : IDisposable {
      ICancellationToken CancellationToken { get; }
   }

   public interface IUserInvocationManager : InvocationManager {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }

   public class InvocationManagerImpl : IUserInvocationManager {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly IInvocationStateFactory invocationStateFactory;
      private readonly PofStreamWriter pofStreamWriter;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IConcurrentDictionary<uint, IInvocationState> pendingInvocationsById;
      
      public InvocationManagerImpl(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, IInvocationStateFactory invocationStateFactory, PofStreamWriter pofStreamWriter) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.invocationStateFactory = invocationStateFactory;
         this.pofStreamWriter = pofStreamWriter;

         this.availableInvocationIds = collectionFactory.CreateUniqueIdentificationSet(true);
         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.pendingInvocationsById = collectionFactory.CreateConcurrentDictionary<uint, IInvocationState>();
      }

      public ICancellationToken CancellationToken { get { return cancellationTokenSource.Token; } }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         var invocationId = availableInvocationIds.TakeUniqueID();
         var invocationState = invocationStateFactory.Create(invocationId, serviceGuid, methodName, methodArguments);
         pendingInvocationsById.Add(invocationId, invocationState);

         throw new NotImplementedException();

         invocationState.Wait();
         return invocationState.Result;
      }

      public void Dispose() {
         this.cancellationTokenSource.Cancel();
      }
   }
}