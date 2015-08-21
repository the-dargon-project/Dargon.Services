using System;
using System.Net;
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

      static ServiceClientFactory() {
         AsyncStatics.__SetInvokerIfUninitialized(() => new AsyncServiceInvokerImpl());
      }

      public IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration) {
         if (clusteringConfiguration.ClusteringRoleFlags == ClusteringRoleFlags.HostOnly &&
             !IPAddress.IsLoopback(clusteringConfiguration.RemoteAddress)) {
            throw new InvalidOperationException("It is impossible host a Dargon Service cluster located at a remote address!");
         }

         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory);
         ClusteringPhaseManager clusteringPhaseManager = new ClusteringPhaseManagerImpl();
         PortableObjectBoxConverter portableObjectBoxConverter = new PortableObjectBoxConverter(streamFactory, pofSerializer);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory, portableObjectBoxConverter);
         ClusteringPhaseFactory clusteringPhaseFactory = new ClusteringPhaseFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, clusteringConfiguration, portableObjectBoxConverter, clusteringPhaseManager);
         ClusteringPhase initialClusteringPhase = clusteringPhaseFactory.CreateIndeterminatePhase(localServiceContainer);
         clusteringPhaseManager.Transition(initialClusteringPhase);
         RemoteServiceInvocationValidatorFactory validatorFactory = new RemoteServiceInvocationValidatorFactoryImpl(collectionFactory);
         RemoteServiceProxyFactory remoteServiceProxyFactory = new RemoteServiceProxyFactoryImpl(proxyGenerator, portableObjectBoxConverter, validatorFactory, clusteringPhaseManager);
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory, portableObjectBoxConverter);
         IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsById = new ConcurrentDictionary<Guid, InvokableServiceContext>();
         LocalServiceRegistry localServiceRegistry = new LocalServiceRegistryImpl(localServiceContainer, clusteringPhaseManager, invokableServiceContextFactory, serviceContextsById);
         IConcurrentDictionary<Type, object> serviceProxiesByInterface = new ConcurrentDictionary<Type, object>();
         RemoteServiceProxyContainer remoteServiceProxyContainer = new RemoteServiceProxyContainerImpl(remoteServiceProxyFactory, serviceProxiesByInterface);
         return new ServiceClientProxyImpl(localServiceRegistry, remoteServiceProxyContainer);
      }
   }
}
