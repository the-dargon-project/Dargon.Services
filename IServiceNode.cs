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
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService;

      public ServiceNode(ICollectionFactory collectionFactory, IServiceNodeContext serviceNodeContext, InvokableServiceContextFactory invokableServiceContextFactory) {
         this.serviceNodeContext = serviceNodeContext;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.serviceContextsByService = collectionFactory.CreateConcurrentDictionary<object, InvokableServiceContext>();
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context = null;
         if (serviceContextsByService.TryAdd(serviceImplementation, () => context = invokableServiceContextFactory.Create(serviceImplementation, serviceInterface))) {
            serviceNodeContext.HandleServiceRegistered(context);
         }
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context;
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
