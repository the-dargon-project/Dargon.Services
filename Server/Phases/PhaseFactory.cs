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
      private readonly IServiceConfiguration serviceConfiguration;
      private readonly IConnectorContext connectorContext;

      public PhaseFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IHostSessionFactory hostSessionFactory, IPofSerializer pofSerializer,  IServiceConfiguration serviceConfiguration, IConnectorContext connectorContext) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.hostSessionFactory = hostSessionFactory;
         this.pofSerializer = pofSerializer;
         this.serviceConfiguration = serviceConfiguration;
         this.connectorContext = connectorContext;
      }

      public IPhase CreateIndeterminatePhase() {
         return new IndeterminatePhase(threadingProxy, networkingProxy, this, serviceConfiguration, connectorContext);
      }

      public IPhase CreateHostPhase(IListenerSocket listenerSocket) {
         var hostContext = new HostContext(connectorContext);
         return new HostPhase(collectionFactory, threadingProxy, networkingProxy, pofSerializer, hostSessionFactory, connectorContext, listenerSocket, hostContext);
      }

      public IPhase CreateGuestPhase(IConnectedSocket clientSocket) {
         return new GuestPhase(collectionFactory, threadingProxy, networkingProxy, this, pofSerializer, connectorContext, clientSocket);
      }
   }
}
