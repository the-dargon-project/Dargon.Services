using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dargon.Services.Clustering.Host {
   public interface IHostSession : IDisposable {
      void Start();
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, object[] arguments);
   }

   public class HostSession : IHostSession, IRemoteInvokable {
      private readonly IHostContext hostContext;
      private readonly IThread thread;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly PofStreamWriter pofStreamWriter;
      private readonly PofDispatcher pofDispatcher;
      private readonly IConcurrentSet<Guid> remotelyHostedServices;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById;

      public HostSession(IHostContext hostContext, IThread thread, ICancellationTokenSource cancellationTokenSource, PofStreamWriter pofStreamWriter, PofDispatcher pofDispatcher, IConcurrentSet<Guid> remotelyHostedServices, IUniqueIdentificationSet availableInvocationIds, IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById) {
         this.hostContext = hostContext;
         this.thread = thread;
         this.cancellationTokenSource = cancellationTokenSource;
         this.pofStreamWriter = pofStreamWriter;
         this.pofDispatcher = pofDispatcher;
         this.remotelyHostedServices = remotelyHostedServices;
         this.availableInvocationIds = availableInvocationIds;
         this.invocationResponseBoxesById = invocationResponseBoxesById;
      }

      public void Initialize() {
         pofDispatcher.RegisterHandler<X2XServiceInvocation>(x => HandleX2XServiceInvocation(x));
         pofDispatcher.RegisterHandler<X2XInvocationResult>(HandleX2XInvocationResult);
         pofDispatcher.RegisterHandler<G2HServiceBroadcast>(HandleG2HServiceBroadcast);
         pofDispatcher.RegisterHandler<G2HServiceUpdate>(HandleG2HServiceUpdate);
      }

      public void Start() {
         pofDispatcher.Start();
      }

      internal async Task HandleX2XServiceInvocation(X2XServiceInvocation x) {
         try {
            var result = await hostContext.Invoke(x.ServiceGuid, x.MethodName, x.MethodArguments);
            var writeTask = pofStreamWriter.WriteAsync(new X2XInvocationResult(x.InvocationId, result));
         } catch (Exception e) {
            Debug.WriteLine(e);
         }
      }

      internal void HandleX2XInvocationResult(X2XInvocationResult x) {
         AsyncValueBox valueBox;
         if (invocationResponseBoxesById.TryGetValue(x.InvocationId, out valueBox)) {
            valueBox.SetResult(x.Payload);
         }
      }

      internal void HandleG2HServiceBroadcast(G2HServiceBroadcast x) {
         HandleServiceUpdateInternal(x.ServiceGuids, null);
      }

      internal void HandleG2HServiceUpdate(G2HServiceUpdate x) {
         HandleServiceUpdateInternal(x.AddedServiceGuids, x.RemovedServiceGuids);
      }

      private void HandleServiceUpdateInternal(IReadOnlySet<Guid> addedServices, IReadOnlySet<Guid> removedServices) {
         addedServices?.ForEach(remotelyHostedServices.Add);
         removedServices?.ForEach(guid => remotelyHostedServices.Remove(guid));

         if (remotelyHostedServices.Count != 0) {
            hostContext.AddRemoteInvokable(this);
         } else {
            hostContext.RemoveRemoteInvokable(this);
         }
      }

      public async Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, object[] arguments) {
         if (!remotelyHostedServices.Contains(serviceGuid)) {
            return new RemoteInvocationResult(false, null);
         } else {
            var invocationId = availableInvocationIds.TakeUniqueID();
            var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, id => new AsyncValueBoxImpl());
            await pofStreamWriter.WriteAsync(new X2XServiceInvocation(invocationId, serviceGuid, methodName, arguments));
            var returnValue = await asyncValueBox.GetResultAsync();
            var removed = invocationResponseBoxesById.Remove(new KeyValuePair<uint, AsyncValueBox>(invocationId, asyncValueBox));
            Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
            return new RemoteInvocationResult(true, returnValue);
         }
      }

      public void Dispose() {
         cancellationTokenSource.Cancel();
         thread.Join();
         thread.Dispose();

         pofDispatcher.Dispose();
         pofStreamWriter.Dispose();
      }
   }
}