using Dargon.PortableObjects.Streams;
using Dargon.Services.Phases;
using Dargon.Services.Phases.Host;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services {
   public interface IServiceClientFactory {
      IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration);
   }

   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;

      public ServiceClientFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, PofStreamsFactory pofStreamsFactory, IHostSessionFactory hostSessionFactory, InvokableServiceContextFactory invokableServiceContextFactory) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofStreamsFactory = pofStreamsFactory;
         this.hostSessionFactory = hostSessionFactory;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
      }

      public IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration) {
         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory);
         ClusteringPhaseManager clusteringPhaseManager = new ClusteringPhaseManagerImpl();
         ClusteringPhaseFactory clusteringPhaseFactory = new ClusteringPhaseFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, clusteringConfiguration, clusteringPhaseManager);
         ClusteringPhase initialClusteringPhase = clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer);
         clusteringPhaseManager.Transition(initialClusteringPhase);
         return new ServiceClient(collectionFactory, localServiceContainer, clusteringPhaseManager, invokableServiceContextFactory);
      }
   }
}
