using Dargon.PortableObjects.Streams;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using System;
using System.Diagnostics;

namespace Dargon.Services.Phases.Guest {
   public class GuestPhase : ClusteringPhase {
      private readonly ClusteringPhaseFactory clusteringPhaseFactory;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly PofStreamWriter pofStreamWriter;
      private readonly PofDispatcher pofDispatcher;

      public GuestPhase(
         PofStreamsFactory pofStreamsFactory, 
         ClusteringPhaseFactory clusteringPhaseFactory, 
         LocalServiceContainer localServiceContainer,
         ClusteringPhaseManager clusteringPhaseManager,
         IConnectedSocket socket
      ) : this(pofStreamsFactory,
               clusteringPhaseFactory,
               localServiceContainer,
               clusteringPhaseManager,
               pofStreamsFactory.CreatePofStream(socket.Stream)
      ) { }

      internal GuestPhase(
         PofStreamsFactory pofStreamsFactory,
         ClusteringPhaseFactory clusteringPhaseFactory, 
         LocalServiceContainer localServiceContainer, 
         ClusteringPhaseManager clusteringPhaseManager,
         PofStream pofStream
      ) : this(
         clusteringPhaseFactory,
         localServiceContainer,
         clusteringPhaseManager,
         pofStream.Writer,
         pofStreamsFactory.CreateDispatcher(pofStream)
      ) { }

      internal GuestPhase(ClusteringPhaseFactory clusteringPhaseFactory, LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, PofStreamWriter pofStreamWriter, PofDispatcher pofDispatcher) {
         this.clusteringPhaseFactory = clusteringPhaseFactory;
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.pofStreamWriter = pofStreamWriter;
         this.pofDispatcher = pofDispatcher;
      }

      public void Initialize() {
         Debug.WriteLine("Guest init");
         pofDispatcher.RegisterHandler<X2XServiceInvocation>(HandleX2XServiceInvocation);
         pofDispatcher.RegisterShutdownHandler(HandleDispatcherShutdown);
         pofDispatcher.Start();
      }

      public void HandleEnter() {
         var servicesGuids = new HashSet<Guid>(localServiceContainer.EnumerateServiceGuids());
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

      private void HandleDispatcherShutdown() {
         clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer));
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         addedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
//         pofSerializer.Serialize(socket.GetWriter(), serviceUpdate);
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         removedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
      }

      public void Dispose() {
         pofDispatcher.Dispose();
         pofStreamWriter.Dispose();
         clusteringPhaseManager.Dispose();
      }
   }
}