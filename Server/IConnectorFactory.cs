using System;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server {
   public interface IConnectorFactory {
      IConnector CreateServiceConnector(IServiceConfiguration serviceConfiguration);
   }

   public class ConnectorFactory : IConnectorFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;

      public ConnectorFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
      }

      public IConnector CreateServiceConnector(IServiceConfiguration serviceConfiguration) {
         IConcurrentDictionary<Guid, IServiceContext> serviceContextsByName = collectionFactory.CreateConcurrentDictionary<Guid, IServiceContext>();
         IConnectorContext connectorContext = new ConnectorContext(collectionFactory, serviceConfiguration);
         IPhase initialPhase = new IndeterminatePhase(threadingProxy, networkingProxy, phaseFactory, connectorContext);
         connectorContext.Transition(initialPhase);
         var connector = new Connector(connectorContext);
         return connector;
      }
   }
}
