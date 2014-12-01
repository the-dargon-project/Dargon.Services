using System;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Events;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Phases {
   public class GuestPhase : IPhase {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly IConnectorContext context;
      private readonly IConnectedSocket socket;
      private readonly IThread readerThread;

      public GuestPhase(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, IPofSerializer pofSerializer, IConnectorContext context, IConnectedSocket socket) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.pofSerializer = pofSerializer;
         this.context = context;
         this.socket = socket;
         this.readerThread = threadingProxy.CreateThread(ReaderThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
      }

      public void Initialize() {
         var handshake = new X2SHandshake(Role.Guest);
         pofSerializer.Serialize(socket.GetWriter().__Writer, handshake);

         readerThread.Start();
      }

      public void RunIteration() {
         var addedServices = collectionFactory.CreateHashSet<Guid>();
         var removedServices = collectionFactory.CreateHashSet<Guid>();

         IConnectorEvent connectorEvent;
         while (context.EventQueue.TryDequeue(out connectorEvent)) {
            var serviceContext = connectorEvent.ServiceContext;
            switch (connectorEvent.Type) {
               case ConnectorEventType.ServiceRegistered:
                  addedServices.Add(serviceContext.Guid);
                  removedServices.Remove(serviceContext.Guid);
                  break;
               case ConnectorEventType.ServiceUnregistered:
                  addedServices.Remove(serviceContext.Guid);
                  removedServices.Add(serviceContext.Guid);
                  break;
            }
         }

         var serviceUpdate = new G2HServiceUpdate(addedServices, removedServices);
         pofSerializer.Serialize(socket.GetWriter().__Writer, serviceUpdate);
      }

      private void ReaderThreadEntryPoint() {
         try {
            while (true) {
               var message = pofSerializer.Deserialize(socket.GetReader().__Reader);
               if (message is H2GServiceInvocation) {
                  ProcessH2GServiceInvocation((H2GServiceInvocation)message);
               }
            }
         } catch (SocketException e) {
            context.Transition(phaseFactory.CreateIndeterminatePhase());
         }
      }

      private void ProcessH2GServiceInvocation(H2GServiceInvocation x) {
         IServiceContext serviceContext;
         object payload;
         if (!context.ServiceContextsByGuid.TryGetValue(x.ServiceGuid, out serviceContext)) {
            payload = new PortableException(new InvalidOperationException("Service Not Found"));
         } else {
            try {
               payload = serviceContext.HandleInvocation(x.MethodName, x.MethodArguments);
            } catch (Exception e) {
               payload = new PortableException(e);
            }
         }
         var result = new G2HInvocationResult(x.InvocationId, payload);
         pofSerializer.Serialize(socket.GetWriter().__Writer, result);
      }

      public void Dispose() {
      }
   }
}