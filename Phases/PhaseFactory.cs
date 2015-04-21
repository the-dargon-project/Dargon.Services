using System;
using Dargon.PortableObjects;
using Dargon.Services.Phases.Guest;
using Dargon.Services.Phases.Host;
using Dargon.Services.Phases.Indeterminate;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Phases {
   public class PhaseFactory : IPhaseFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IPofSerializer pofSerializer;

      public PhaseFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IHostSessionFactory hostSessionFactory, IPofSerializer pofSerializer) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.hostSessionFactory = hostSessionFactory;
         this.pofSerializer = pofSerializer;
      }

      public IPhase CreateIndeterminatePhase(IServiceNodeContext serviceNodeContext) {
         var phase = new IndeterminatePhase(threadingProxy, networkingProxy, this, serviceNodeContext);
         return phase;
      }

      public IPhase CreateHostPhase(IServiceNodeContext serviceNodeContext, IListenerSocket listenerSocket) {
         var hostContext = new HostContext(serviceNodeContext);
         var phase = new HostPhase(collectionFactory, threadingProxy, hostSessionFactory, hostContext, listenerSocket);
         return phase;
      }

      public IPhase CreateGuestPhase(IServiceNodeContext serviceNodeContext, IConnectedSocket clientSocket) {
         var phase = new GuestPhase(collectionFactory, threadingProxy, networkingProxy, this, pofSerializer, serviceNodeContext, clientSocket);
         return phase;
      }
   }
}
