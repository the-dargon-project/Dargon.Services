using Dargon.Services.Phases;
using Dargon.Services.Server;
using ItzWarty;
using ItzWarty.Collections;
using System;

namespace Dargon.Services {
   public interface IServiceClient : IDisposable {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);
   }

   public class ServiceClient : IServiceClient {
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService;

      public ServiceClient(
         ICollectionFactory collectionFactory,
         LocalServiceContainer localServiceContainer,
         ClusteringPhaseManager clusteringPhaseManager,
         InvokableServiceContextFactory invokableServiceContextFactory
      ) : this(
         localServiceContainer,
         clusteringPhaseManager,
         invokableServiceContextFactory,
         collectionFactory.CreateConcurrentDictionary<object, InvokableServiceContext>()
      ) { }

      public ServiceClient(LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, InvokableServiceContextFactory invokableServiceContextFactory, IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService) {
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.serviceContextsByService = serviceContextsByService;
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

      public void Dispose() { }
   }
}
