using System;
using System.Linq;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
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

      public void HandleEnter() {
         var handshake = new X2SHandshake(Role.Guest);
         pofSerializer.Serialize(socket.GetWriter().__Writer, handshake);

         var servicesGuids = new HashSet<Guid>(context.ServiceContextsByGuid.Keys);
         pofSerializer.Serialize(socket.GetWriter().__Writer, new G2HServiceBroadcast(servicesGuids));

         readerThread.Start();
      }

      private void ReaderThreadEntryPoint() {
         try {
            while (true) {
               var message = pofSerializer.Deserialize(socket.GetReader().__Reader);
               if (message is X2XServiceInvocation) {
                  ProcessH2GServiceInvocation((X2XServiceInvocation)message);
               }
            }
         } catch (SocketException e) {
            context.Transition(phaseFactory.CreateIndeterminatePhase(context));
         }
      }

      private void ProcessH2GServiceInvocation(X2XServiceInvocation x) {
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
         var result = new X2XInvocationResult(x.InvocationId, payload);
         pofSerializer.Serialize(socket.GetWriter().__Writer, result);
      }

      public void Dispose() {
      }

      public void HandleServiceRegistered(IServiceContext serviceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         addedServices.Add(serviceContext.Guid);
         var serviceUpdate = new G2HServiceUpdate(addedServices, removedServices);
         pofSerializer.Serialize(socket.GetWriter().__Writer, serviceUpdate);
      }

      public void HandleServiceUnregistered(IServiceContext serviceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         removedServices.Add(serviceContext.Guid);
         var serviceUpdate = new G2HServiceUpdate(addedServices, removedServices);
         pofSerializer.Serialize(socket.GetWriter().__Writer, serviceUpdate);
      }
   }
}