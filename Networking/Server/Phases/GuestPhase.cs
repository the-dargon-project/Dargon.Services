using System;
using System.IO;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.Networking.Events;
using Dargon.Services.Networking.PortableObjects;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Networking.Server.Phases {
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
         pofSerializer.Serialize(socket.GetWriter(), handshake);

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
         pofSerializer.Serialize(socket.GetWriter(), serviceUpdate);
      }

      private void ReaderThreadEntryPoint() {
         try {
            while (true) {
               var serviceInvocation = pofSerializer.Deserialize<H2GServiceInvocation>(socket.GetReader());
               IServiceContext serviceContext;
               if (!context.ServiceContextsByGuid.TryGetValue(serviceInvocation.ServiceGuid, out serviceContext)) {
               } else {
                  var result = serviceContext.HandleInvocation(serviceInvocation.MethodName, serviceInvocation.MethodArguments);
                  pofSerializer.Serialize;
               }
            }
         } catch (SocketException e) {
            context.Transition(phaseFactory.CreateIndeterminatePhase());
         }
      }

      public void Dispose() {
      }
   }
}