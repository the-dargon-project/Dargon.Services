using Dargon.Services.Server;
using ItzWarty;
using ItzWarty.Collections;
using System;

namespace Dargon.Services {
   public interface IServiceNode : IDisposable {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);
   }

   public class ServiceNode : IServiceNode {
      private readonly IServiceNodeContext serviceNodeContext;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConcurrentDictionary<object, IServiceContext> serviceContextsByService;

      public ServiceNode(ICollectionFactory collectionFactory, IServiceNodeContext serviceNodeContext, IServiceContextFactory serviceContextFactory) {
         this.serviceNodeContext = serviceNodeContext;
         this.serviceContextFactory = serviceContextFactory;
         this.serviceContextsByService = collectionFactory.CreateConcurrentDictionary<object, IServiceContext>();
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         IServiceContext context = null;
         if (serviceContextsByService.TryAdd(serviceImplementation, () => context = serviceContextFactory.Create(serviceImplementation, serviceInterface))) {
            serviceNodeContext.HandleServiceRegistered(context);
         }
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         IServiceContext context;
         if (serviceContextsByService.TryGetValue(serviceImplementation, out context)) {
            if (serviceContextsByService.TryRemove(serviceImplementation, context)) {
               serviceNodeContext.HandleServiceUnregistered(context);
            }
         }
      }

      public void Dispose() {
         serviceNodeContext.Dispose();
      }
   }
}
