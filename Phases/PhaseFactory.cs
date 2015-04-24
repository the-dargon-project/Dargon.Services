using System;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
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
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IPofSerializer pofSerializer;

      public PhaseFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, PofStreamsFactory pofStreamsFactory, IHostSessionFactory hostSessionFactory, IPofSerializer pofSerializer) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofStreamsFactory = pofStreamsFactory;
         this.hostSessionFactory = hostSessionFactory;
         this.pofSerializer = pofSerializer;
      }

      public IPhase CreateIndeterminatePhase(LocalServiceContainer localServiceContainer) {
         var phase = new IndeterminatePhase(threadingProxy, networkingProxy, this, localServiceContainer);
         return phase;
      }

      public IPhase CreateHostPhase(LocalServiceContainer localServiceContainer, IListenerSocket listenerSocket) {
         var hostContext = new HostContext(localServiceContainer);
         var phase = new HostPhase(collectionFactory, threadingProxy, hostSessionFactory, hostContext, listenerSocket);
         return phase;
      }

      public IPhase CreateGuestPhase(LocalServiceContainer localServiceContainer, IConnectedSocket clientSocket) {
         var phase = new GuestPhase(pofStreamsFactory, this, pofSerializer, localServiceContainer, clientSocket);
         phase.Initialize();
         return phase;
      }
   }
}
