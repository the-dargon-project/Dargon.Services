using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Dargon.Services.Clustering.Host {
   public interface IHostSession : IDisposable {
      Task StartAndAwaitShutdown();
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, object[] arguments);
   }

   public class HostSession : IHostSession, IRemoteInvokable {
      private readonly IHostContext hostContext;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly MessageSender messageSender;
      private readonly PofDispatcher pofDispatcher;
      private readonly IConcurrentSet<Guid> remotelyHostedServices;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById;
      private readonly AsyncManualResetEvent shutdownLatch = new AsyncManualResetEvent();

      public HostSession(IHostContext hostContext, ICancellationTokenSource cancellationTokenSource, MessageSender messageSender, PofDispatcher pofDispatcher, IConcurrentSet<Guid> remotelyHostedServices, IUniqueIdentificationSet availableInvocationIds, IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById) {
         this.hostContext = hostContext;
         this.cancellationTokenSource = cancellationTokenSource;
         this.messageSender = messageSender;
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
         pofDispatcher.RegisterShutdownHandler(HandleDispatcherShutdown);
      }

      private void HandleDispatcherShutdown() {
         shutdownLatch.Set();
      }

      public async Task StartAndAwaitShutdown() {
         pofDispatcher.Start();
         await shutdownLatch.WaitAsync();
      }

      internal async Task HandleX2XServiceInvocation(X2XServiceInvocation x) {
         try {
            var result = await hostContext.Invoke(x.ServiceGuid, x.MethodName, x.MethodArguments);
            var sendTask = messageSender.SendInvocationResultAsync(x.InvocationId, result);
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
            await messageSender.SendServiceInvocationAsync(invocationId, serviceGuid, methodName, arguments);
            var returnValue = await asyncValueBox.GetResultAsync();
            var removed = invocationResponseBoxesById.Remove(new KeyValuePair<uint, AsyncValueBox>(invocationId, asyncValueBox));
            Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
            return new RemoteInvocationResult(true, returnValue);
         }
      }

      public void Dispose() {
         cancellationTokenSource.Cancel();
         shutdownLatch.Set();

         pofDispatcher.Dispose();
         messageSender.Dispose();
      }
   }
}