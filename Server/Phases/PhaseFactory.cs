using Dargon.PortableObjects;
using Dargon.Services.Server.Sessions;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Phases {
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

      public IPhase CreateIndeterminatePhase(IConnectorContext connectorContext) {
         var phase = new IndeterminatePhase(threadingProxy, networkingProxy, this, connectorContext);
         return phase;
      }

      public IPhase CreateHostPhase(IConnectorContext connectorContext, IListenerSocket listenerSocket) {
         var hostContext = new HostContext(connectorContext);
         var phase = new HostPhase(collectionFactory, threadingProxy, networkingProxy, pofSerializer, hostSessionFactory, connectorContext, listenerSocket, hostContext);
         return phase;
      }

      public IPhase CreateGuestPhase(IConnectorContext connectorContext, IConnectedSocket clientSocket) {
         var phase = new GuestPhase(collectionFactory, threadingProxy, networkingProxy, this, pofSerializer, connectorContext, clientSocket);
         return phase;
      }
   }
}
