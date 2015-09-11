using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using Nito.AsyncEx;
using NLog;

namespace Dargon.Services.Clustering.Local.Host {
   public interface HostSession : IDisposable {
      Task StartAndAwaitShutdown();
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] arguments);
   }

   public class HostSessionImpl : HostSession, RemoteInvokable {
      private readonly static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly HostContext hostContext;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly MessageSender messageSender;
      private readonly PofDispatcher pofDispatcher;
      private readonly IConcurrentSet<Guid> remotelyHostedServices;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById;
      private readonly AsyncManualResetEvent shutdownLatch = new AsyncManualResetEvent();

      public HostSessionImpl(HostContext hostContext, ICancellationTokenSource cancellationTokenSource, MessageSender messageSender, PofDispatcher pofDispatcher, IConcurrentSet<Guid> remotelyHostedServices, IUniqueIdentificationSet availableInvocationIds, IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById) {
         this.hostContext = hostContext;
         this.cancellationTokenSource = cancellationTokenSource;
         this.messageSender = messageSender;
         this.pofDispatcher = pofDispatcher;
         this.remotelyHostedServices = remotelyHostedServices;
         this.availableInvocationIds = availableInvocationIds;
         this.invocationResponseBoxesById = invocationResponseBoxesById;
      }

      public void Initialize() {
         pofDispatcher.RegisterHandler<X2XServiceInvocation>(HandleX2XServiceInvocation);
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

      internal void HandleX2XServiceInvocation(X2XServiceInvocation x) {
         logger.Trace($"Invoking service {x.ServiceGuid} method {x.MethodName} with {x.MethodArguments.Length} arguments");
         Task.Factory.StartNew(async (dummy) => {
            try {
               var result = await hostContext.Invoke(x.ServiceGuid, x.MethodName, x.GenericArguments, x.MethodArguments);
               var sendTask = messageSender.SendInvocationResultAsync(x.InvocationId, result);
            } catch (Exception e) {
               logger.Error(e);
            }
         }, CancellationToken.None, TaskCreationOptions.LongRunning);
      }

      internal void HandleX2XInvocationResult(X2XInvocationResult x) {
         logger.Trace($"Handling invocation result for iid {x.InvocationId} result length {x.PayloadBox.Length}");
         AsyncValueBox valueBox;
         if (invocationResponseBoxesById.TryGetValue(x.InvocationId, out valueBox)) {
            valueBox.SetResult(x.PayloadBox);
         }
      }

      internal void HandleG2HServiceBroadcast(G2HServiceBroadcast x) {
         logger.Trace($"Received broadcast of services {x.ServiceGuids.Join(" ")}.");
         HandleServiceUpdateInternal(x.ServiceGuids, null);
      }

      internal void HandleG2HServiceUpdate(G2HServiceUpdate x) {
         logger.Trace($"Received update: add {x.AddedServiceGuids.Join(" ")}, removed {x.RemovedServiceGuids.Join(" ")}.");
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

      public Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] arguments) {
         logger.Trace($"Remote invoking service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {arguments.Length} arguments.");
         if (!remotelyHostedServices.Contains(serviceGuid)) {
            return Task.FromResult(new RemoteInvocationResult(false, null));
         } else {
            return Task.Factory.StartNew(async (throwaway) => {
               var invocationId = availableInvocationIds.TakeUniqueID();
               var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, id => new AsyncValueBoxImpl());
               await messageSender.SendServiceInvocationAsync(invocationId, serviceGuid, methodName, genericArguments, arguments);
               var returnValue = await asyncValueBox.GetResultAsync();
               var removed = invocationResponseBoxesById.Remove(new KeyValuePair<uint, AsyncValueBox>(invocationId, asyncValueBox));
               Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
               return new RemoteInvocationResult(true, returnValue);
            }, CancellationToken.None, TaskCreationOptions.LongRunning).Unwrap();
         }
      }

      public Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, PortableObjectBox genericArguments, PortableObjectBox argumentsDto) {
         logger.Trace($"Remote invoking service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {argumentsDto.Length} bytes of arguments.");
         if (!remotelyHostedServices.Contains(serviceGuid)) {
            logger.Trace($"Remote does not have service {serviceGuid}.");
            return Task.FromResult(new RemoteInvocationResult(false, null));
         } else {
            logger.Trace($"Remote has service {serviceGuid}.");
            return Task.Factory.StartNew(async (throwaway) => {
               var invocationId = availableInvocationIds.TakeUniqueID();
               logger.Trace($"Took iid {invocationId} for service {serviceGuid} method {methodName}.");
               var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, id => new AsyncValueBoxImpl());
               await messageSender.SendServiceInvocationAsync(invocationId, serviceGuid, methodName, genericArguments, argumentsDto);
               var returnValue = await asyncValueBox.GetResultAsync();
               logger.Trace($"Got result for iid {invocationId} service {serviceGuid} method {methodName}.");
               var removed = invocationResponseBoxesById.Remove(new KeyValuePair<uint, AsyncValueBox>(invocationId, asyncValueBox));
               Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
               return new RemoteInvocationResult(true, returnValue);
            }, CancellationToken.None, TaskCreationOptions.LongRunning).Unwrap();
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