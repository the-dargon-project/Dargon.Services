using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using Dargon.Services.Clustering;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services {
   public interface IServiceClientFactory {
      IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration);
   }

   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ProxyGenerator proxyGenerator;
      private readonly IStreamFactory streamFactory;
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;

      public ServiceClientFactory(ProxyGenerator proxyGenerator, IStreamFactory streamFactory, ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, PofStreamsFactory pofStreamsFactory) {
         this.proxyGenerator = proxyGenerator;
         this.streamFactory = streamFactory;
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
      }

      public IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration) {
         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory);
         ClusteringPhaseManager clusteringPhaseManager = new ClusteringPhaseManagerImpl();
         MethodArgumentsConverter methodArgumentsConverter = new MethodArgumentsConverter(streamFactory, pofSerializer);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory, methodArgumentsConverter);
         ClusteringPhaseFactory clusteringPhaseFactory = new ClusteringPhaseFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, clusteringConfiguration, methodArgumentsConverter, clusteringPhaseManager);
         ClusteringPhase initialClusteringPhase = clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer);
         clusteringPhaseManager.Transition(initialClusteringPhase);
         RemoteServiceInvocationValidatorFactory validatorFactory = new RemoteServiceInvocationValidatorFactoryImpl(collectionFactory);
         RemoteServiceProxyFactory remoteServiceProxyFactory = new RemoteServiceProxyFactoryImpl(proxyGenerator, validatorFactory, clusteringPhaseManager);
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory, methodArgumentsConverter);
         return new ServiceClient(collectionFactory, localServiceContainer, clusteringPhaseManager, invokableServiceContextFactory, remoteServiceProxyFactory);
      }
   }
}
