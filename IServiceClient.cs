using Dargon.Services.Server;
using ItzWarty;
using ItzWarty.Collections;
using System;
using Dargon.Services.Client;
using Dargon.Services.Clustering;

namespace Dargon.Services {
   public interface IServiceClient : IDisposable {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);

      TService GetService<TService>() where TService : class;
   }

   public class ServiceClient : IServiceClient {
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly RemoteServiceProxyFactory remoteServiceProxyFactory;
      private readonly IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService;
      private readonly IConcurrentDictionary<Type, object> serviceProxiesByInterface;

      public ServiceClient(
         ICollectionFactory collectionFactory,
         LocalServiceContainer localServiceContainer,
         ClusteringPhaseManager clusteringPhaseManager,
         InvokableServiceContextFactory invokableServiceContextFactory,
         RemoteServiceProxyFactory remoteServiceProxyFactory
      ) : this(
         localServiceContainer,
         clusteringPhaseManager,
         invokableServiceContextFactory,
         remoteServiceProxyFactory,
         collectionFactory.CreateConcurrentDictionary<object, InvokableServiceContext>(),
         collectionFactory.CreateConcurrentDictionary<Type, object>()
      ) { }

      public ServiceClient(LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, InvokableServiceContextFactory invokableServiceContextFactory, RemoteServiceProxyFactory remoteServiceProxyFactory, IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService, IConcurrentDictionary<Type, object> serviceProxiesByInterface) {
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.remoteServiceProxyFactory = remoteServiceProxyFactory;
         this.serviceContextsByService = serviceContextsByService;
         this.serviceProxiesByInterface = serviceProxiesByInterface;
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context = null;
         if (serviceContextsByService.TryAdd(serviceImplementation, () => context = invokableServiceContextFactory.Create(serviceImplementation, serviceInterface))) {
            localServiceContainer.Register(context);
            clusteringPhaseManager.HandleServiceRegistered(context);
         }
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context;
         if (serviceContextsByService.TryGetValue(serviceImplementation, out context)) {
            if (serviceContextsByService.TryRemove(serviceImplementation, context)) {
               localServiceContainer.Unregister(context);
               clusteringPhaseManager.HandleServiceUnregistered(context);
            }
         }
      }

      public TService GetService<TService>() where TService : class {
         return (TService)serviceProxiesByInterface.GetOrAdd(
            typeof(TService),
            add => remoteServiceProxyFactory.Create<TService>()
         );
      }

      public void Dispose() { }
   }
}
