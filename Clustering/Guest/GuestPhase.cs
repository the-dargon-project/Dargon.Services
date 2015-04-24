using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Clustering.Host;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Networking;

namespace Dargon.Services.Clustering.Guest {
   public class GuestPhase : ClusteringPhase {
      private readonly ClusteringPhaseFactory clusteringPhaseFactory;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly PofStreamWriter pofStreamWriter;
      private readonly PofDispatcher pofDispatcher;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById;

      public GuestPhase(
         ICollectionFactory collectionFactory,
         PofStreamsFactory pofStreamsFactory, 
         ClusteringPhaseFactory clusteringPhaseFactory, 
         LocalServiceContainer localServiceContainer,
         ClusteringPhaseManager clusteringPhaseManager,
         IConnectedSocket socket
      ) : this(pofStreamsFactory,
               clusteringPhaseFactory,
               localServiceContainer,
               clusteringPhaseManager,
               pofStreamsFactory.CreatePofStream(socket.Stream),
               collectionFactory.CreateUniqueIdentificationSet(true),
               collectionFactory.CreateConcurrentDictionary<uint, AsyncValueBox>()
      ) { }

      internal GuestPhase(
         PofStreamsFactory pofStreamsFactory,
         ClusteringPhaseFactory clusteringPhaseFactory, 
         LocalServiceContainer localServiceContainer, 
         ClusteringPhaseManager clusteringPhaseManager,
         PofStream pofStream,
         IUniqueIdentificationSet availableInvocationIds,
         IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById
      ) : this(
         clusteringPhaseFactory,
         localServiceContainer,
         clusteringPhaseManager,
         pofStream.Writer,
         pofStreamsFactory.CreateDispatcher(pofStream),
         availableInvocationIds,
         invocationResponseBoxesById
      ) { }

      internal GuestPhase(ClusteringPhaseFactory clusteringPhaseFactory, LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, PofStreamWriter pofStreamWriter, PofDispatcher pofDispatcher, IUniqueIdentificationSet availableInvocationIds, IConcurrentDictionary<uint, AsyncValueBox> invocationResponseBoxesById) {
         this.clusteringPhaseFactory = clusteringPhaseFactory;
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.pofStreamWriter = pofStreamWriter;
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
         var servicesGuids = new ItzWarty.Collections.HashSet<Guid>(localServiceContainer.EnumerateServiceGuids());
         pofStreamWriter.WriteAsync(new G2HServiceBroadcast(servicesGuids));
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
         pofStreamWriter.WriteAsync(new X2XInvocationResult(x.InvocationId, result));
      }

      internal void HandleX2XInvocationResult(X2XInvocationResult x) {
         AsyncValueBox valueBox;
         if (invocationResponseBoxesById.TryGetValue(x.InvocationId, out valueBox)) {
            valueBox.SetResult(x.Payload);
         }
      }

      private void HandleDispatcherShutdown() {
         clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer));
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new ItzWarty.Collections.HashSet<Guid>();
         var removedServices = new ItzWarty.Collections.HashSet<Guid>();
         addedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
//         pofSerializer.Serialize(socket.GetWriter(), serviceUpdate);
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new ItzWarty.Collections.HashSet<Guid>();
         var removedServices = new ItzWarty.Collections.HashSet<Guid>();
         removedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
      }

      public async Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, object[] methodArguments) {
         object localResult;
         if (localServiceContainer.TryInvoke(serviceGuid, methodName, methodArguments, out localResult)) {
            return localResult;
         } else {
            // Code looks different than in host session - if an exception has been thrown
            var invocationId = availableInvocationIds.TakeUniqueID();
            var asyncValueBox = invocationResponseBoxesById.GetOrAdd(invocationId, (id) => new AsyncValueBoxImpl());
            await pofStreamWriter.WriteAsync(new X2XServiceInvocation(invocationId, serviceGuid, methodName, methodArguments));
            var returnValue = await asyncValueBox.GetResultAsync();
            var removed = invocationResponseBoxesById.Remove(invocationId.PairValue(asyncValueBox));
            Trace.Assert(removed, "Failed to remove AsyncValueBox from dict");
            return returnValue;
         }
      }

      public void Dispose() {
         pofDispatcher.Dispose();
         pofStreamWriter.Dispose();
         clusteringPhaseManager.Dispose();
      }
   }
}