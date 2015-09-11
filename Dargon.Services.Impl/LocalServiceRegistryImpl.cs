using System;
using Dargon.Services.Clustering.Local;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services {
   public class LocalServiceRegistryImpl : LocalServiceRegistry {
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsById;

      public LocalServiceRegistryImpl(LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager, InvokableServiceContextFactory invokableServiceContextFactory, IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsById) {
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.serviceContextsById = serviceContextsById;
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface, Guid guid) {
         if (serviceImplementation == null) {
            throw new ArgumentNullException(nameof(serviceImplementation));
         } else if (serviceInterface == null) {
            throw new ArgumentNullException(nameof(serviceInterface));
         } else if (!serviceInterface.IsInterface) {
            throw new ArgumentException($"Provided Service Interface '{serviceInterface.FullName}' was not an interface!");
         } else if (guid.Equals(Guid.Empty)) {
            throw new ArgumentException($"Provided guid for service '{serviceInterface.FullName}' was zero guid!");
         } else {
            InvokableServiceContext context = invokableServiceContextFactory.Create(serviceImplementation, serviceInterface, guid);
            if (serviceContextsById.TryAdd(guid, context)) {
               localServiceContainer.Register(context);
               clusteringPhaseManager.HandleServiceRegistered(context);
            }
         }
      }

      public void UnregisterService(Guid serviceGuid) {
         if (serviceGuid.Equals(Guid.Empty)) {
            throw new ArgumentException($"Provided guid for removed service was zero guid!");
         } else {
            InvokableServiceContext context;
            if (serviceContextsById.TryRemove(serviceGuid, out context)) {
               localServiceContainer.Unregister(context);
               clusteringPhaseManager.HandleServiceUnregistered(context);
            }
         }
      }
   }
}