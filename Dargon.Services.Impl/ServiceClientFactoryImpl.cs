using System;
using System.Net;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using Dargon.Services.Clustering;
using Dargon.Services.Clustering.Local;
using Dargon.Services.Clustering.Local.Host;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services {
   public class ServiceClientFactoryImpl : ServiceClientFactory {
      private readonly ProxyGenerator proxyGenerator;
      private readonly IStreamFactory streamFactory;
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;

      public ServiceClientFactoryImpl(ProxyGenerator proxyGenerator, IStreamFactory streamFactory, ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, PofStreamsFactory pofStreamsFactory) {
         this.proxyGenerator = proxyGenerator;
         this.streamFactory = streamFactory;
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
      }

      static ServiceClientFactoryImpl() {
         AsyncStatics.__SetInvokerIfUninitialized(() => new AsyncServiceInvokerImpl());
      }

      public ServiceClient Local(int port) {
         return Local(port, ClusteringRole.HostOrGuest);
      }

      public ServiceClient Local(int port, ClusteringRole clusteringRole) {
         return Construct(new ClusteringConfigurationImpl(
            IPAddress.Loopback,
            port,
            clusteringRole));
      }

      public ServiceClient Remote(IPEndPoint endpoint) {
         return Remote(endpoint.Address, endpoint.Port);
      }

      public ServiceClient Remote(IPAddress address, int port) {
         return Construct(new ClusteringConfigurationImpl(
            address,
            port,
            ClusteringRole.GuestOnly));
      }

      public ServiceClient Construct(ClusteringConfiguration clusteringConfiguration) {
         if (clusteringConfiguration.ClusteringRole == ClusteringRole.HostOnly &&
             !IPAddress.IsLoopback(clusteringConfiguration.Address)) {
            throw new InvalidOperationException("It is impossible host a Dargon Service cluster located at a remote address!");
         }

         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory);
         ClusteringPhaseManager clusteringPhaseManager = new ClusteringPhaseManagerImpl();
         PortableObjectBoxConverter portableObjectBoxConverter = new PortableObjectBoxConverter(streamFactory, pofSerializer);
         HostSessionFactory hostSessionFactory = new HostSessionFactoryImpl(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory, portableObjectBoxConverter);
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