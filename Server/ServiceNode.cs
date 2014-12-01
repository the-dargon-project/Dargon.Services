using System;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public class ServiceNode : IServiceNode {
      private readonly IConnector connector;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConcurrentDictionary<object, IServiceContext> serviceContextsByService; 

      public ServiceNode(ICollectionFactory collectionFactory, IConnector connector, IServiceContextFactory serviceContextFactory) {
         this.connector = connector;
         this.serviceContextFactory = serviceContextFactory;
         this.serviceContextsByService = collectionFactory.CreateConcurrentDictionary<object, IServiceContext>();
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         IServiceContext context = null;
         if (serviceContextsByService.TryAdd(serviceImplementation, () => context = serviceContextFactory.Create(serviceImplementation, serviceInterface))) {
            connector.RegisterService(context);
         }
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         IServiceContext context;
         if (serviceContextsByService.TryGetValue(serviceImplementation, out context)) {
            if (serviceContextsByService.TryRemove(serviceImplementation, context)) {
               connector.UnregisterService(context);
            }
         }
      }

      public void Dispose() {
         connector.Dispose();
      }
   }
}
