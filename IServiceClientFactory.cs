using Castle.DynamicProxy;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using Dargon.Services.Clustering;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services {
   public interface IServiceClientFactory {
      IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration);
   }

   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ProxyGenerator proxyGenerator;
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;

      public ServiceClientFactory(ProxyGenerator proxyGenerator, ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, PofStreamsFactory pofStreamsFactory, IHostSessionFactory hostSessionFactory, InvokableServiceContextFactory invokableServiceContextFactory) {
         this.proxyGenerator = proxyGenerator;
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
         RemoteServiceInvocationValidatorFactory validatorFactory = new RemoteServiceInvocationValidatorFactoryImpl(collectionFactory);
         RemoteServiceProxyFactory remoteServiceProxyFactory = new RemoteServiceProxyFactoryImpl(proxyGenerator, validatorFactory, clusteringPhaseManager);
         return new ServiceClient(collectionFactory, localServiceContainer, clusteringPhaseManager, invokableServiceContextFactory, remoteServiceProxyFactory);
      }
   }
}
