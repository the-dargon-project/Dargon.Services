using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Networking;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dargon.Services.Clustering.Guest {
   public class GuestPhase : ClusteringPhase {
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
         Debug.WriteLine("Guest init");
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
         object result;
         try {
            if (!localServiceContainer.TryInvoke(x.ServiceGuid, x.MethodName, x.MethodArguments, out result)) {
               result = new PortableException(new ServiceUnavailableException(x.ServiceGuid, x.MethodName));
            }
         } catch (Exception e) {
            result = new PortableException(e);
         }
         messageSender.SendInvocationResultAsync(x.InvocationId, result);
      }

      internal void HandleX2XInvocationResult(X2XInvocationResult x) {
         AsyncValueBox valueBox;
         if (invocationResponseBoxesById.TryGetValue(x.InvocationId, out valueBox)) {
            valueBox.SetResult(x.PayloadBox);
         }
      }

      private void HandleDispatcherShutdown() {
         clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer));
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid> { invokableServiceContext.Guid };
         var removedServices = new HashSet<Guid>();
         messageSender.SendServiceUpdateAsync(addedServices, removedServices);
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid> { invokableServiceContext.Guid };
         messageSender.SendServiceUpdateAsync(addedServices, removedServices);
      }

      public async Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, object[] methodArguments) {
         object localResult;
         if (localServiceContainer.TryInvoke(serviceGuid, methodName, methodArguments, out localResult)) {
            return localResult;
         } else {
            // Code looks different than in host session - if an exception has been thrown
            var invocationId = availableInvocationIds.TakeUniqueID();
            var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, (id) => new AsyncValueBoxImpl());
            await messageSender.SendServiceInvocationAsync(invocationId, serviceGuid, methodName, methodArguments);
            var returnValue = await asyncValueBox.GetResultAsync();
            var removed = invocationResponseBoxesById.Remove(invocationId.PairValue(asyncValueBox));
            Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
            return returnValue;
         }
      }

      public void Dispose() {
         messageSender.Dispose();
         pofDispatcher.Dispose();
         clusteringPhaseManager.Dispose();
      }
   }
}