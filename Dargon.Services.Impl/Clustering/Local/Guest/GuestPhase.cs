using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Services.Clustering.Local.Guest {
   public class GuestPhase : ClusteringPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ClusteringPhaseFactory clusteringPhaseFactory;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly MessageSender messageSender;
      private readonly PofDispatcher pofDispatcher;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById;

      public GuestPhase(ClusteringPhaseFactory clusteringPhaseFactory, LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, MessageSender messageSender, PofDispatcher pofDispatcher, IUniqueIdentificationSet availableInvocationIds, IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById) {
         this.clusteringPhaseFactory = clusteringPhaseFactory;
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.messageSender = messageSender;
         this.pofDispatcher = pofDispatcher;
         this.availableInvocationIds = availableInvocationIds;
         this.invocationResponseBoxesById = invocationResponseBoxesById;
      }

      public void Initialize() {
         logger.Info("Guest init");
         pofDispatcher.RegisterHandler<X2XServiceInvocation>(HandleX2XServiceInvocation);
         pofDispatcher.RegisterHandler<X2XInvocationResult>(HandleX2XInvocationResult);
         pofDispatcher.RegisterShutdownHandler(HandleDispatcherShutdown);
         pofDispatcher.Start();
      }

      public void HandleEnter() {
         var servicesGuids = new HashSet<Guid>(localServiceContainer.EnumerateServiceGuids());
         messageSender.SendServiceBroadcastAsync(servicesGuids);
      }

      private void HandleX2XServiceInvocation(X2XServiceInvocation x) {
         logger.Trace($"{nameof(HandleX2XServiceInvocation)} for service {x.ServiceGuid} iid {x.InvocationId} method {x.MethodName}");
         Task.Factory.StartNew((dummy) => {
            object result;
            try {
               if (localServiceContainer.TryInvoke(x.ServiceGuid, x.MethodName, x.GenericArguments, x.MethodArguments, out result)) {
                  logger.Trace($"Successfully locally invoked service {x.ServiceGuid} method {x.MethodName} for iid {x.InvocationId}.");
               } else {
                  logger.Trace($"Could not locally find service {x.ServiceGuid} method {x.MethodName} for iid {x.InvocationId}.");
                  result = new PortableException(new ServiceUnavailableException(x.ServiceGuid, x.MethodName));
               }
            } catch (Exception e) {
               logger.Trace($"Local invocation for service {x.ServiceGuid} method {x.MethodName} for iid {x.InvocationId} threw ", e);
               if (e is IPortableObject) {
                  result = e;
               } else {
                  result = new PortableException(e);
               }
            }
            messageSender.SendInvocationResultAsync(x.InvocationId, result);
         }, CancellationToken.None, TaskCreationOptions.LongRunning);
      }

      internal void HandleX2XInvocationResult(X2XInvocationResult x) {
         logger.Trace($"{nameof(HandleX2XInvocationResult)} for iid {x.InvocationId} payload {x.PayloadBox.Length}");
         AsyncValueBox valueBox;
         if (invocationResponseBoxesById.TryGetValue(x.InvocationId, out valueBox)) {
            valueBox.SetResult(x.PayloadBox);
         }
      }

      private void HandleDispatcherShutdown() {
         clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer));
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         logger.Trace($"Locally registered service {invokableServiceContext.Guid}.");
         var addedServices = new HashSet<Guid> { invokableServiceContext.Guid };
         var removedServices = new HashSet<Guid>();
         messageSender.SendServiceUpdateAsync(addedServices, removedServices);
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         logger.Trace($"Locally unregistered service {invokableServiceContext.Guid}.");
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid> { invokableServiceContext.Guid };
         messageSender.SendServiceUpdateAsync(addedServices, removedServices);
      }

      public Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments) {
         logger.Trace($"Invoking service {serviceGuid} method {methodName} with {methodArguments.Length} arguments");
         return Task.Factory.StartNew(async (throwaway) => {
            object localResult;
            if (localServiceContainer.TryInvoke(serviceGuid, methodName, genericArguments, methodArguments, out localResult)) {
               return localResult;
            } else {
               // Code looks different than in host session - if an exception has been thrown
               var invocationId = availableInvocationIds.TakeUniqueID();
               logger.Trace($"Took invocationId {invocationId} for {serviceGuid} method {methodName} call.");
               var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, (id) => new AsyncValueBoxImpl());
               await messageSender.SendServiceInvocationAsync(invocationId, serviceGuid, methodName, genericArguments, methodArguments);
               var returnValue = await asyncValueBox.GetResultAsync();
               var removed = invocationResponseBoxesById.Remove(invocationId.PairValue(asyncValueBox));
               Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
               return returnValue;
            }
         }, CancellationToken.None, TaskCreationOptions.LongRunning).Unwrap();
      }

      public void Dispose() {
         messageSender.Dispose();
         pofDispatcher.Dispose();
         clusteringPhaseManager.Dispose();
      }
   }
}