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
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IConcurrentDictionary<object, InvokableServiceContext> serviceContextsByService;

      public ServiceClient(ICollectionFactory collectionFactory, LocalServiceContainer localServiceContainer, InvokableServiceContextFactory invokableServiceContextFactory) {
         this.localServiceContainer = localServiceContainer;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.serviceContextsByService = collectionFactory.CreateConcurrentDictionary<object, InvokableServiceContext>();
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context = null;
         if (serviceContextsByService.TryAdd(serviceImplementation, () => context = invokableServiceContextFactory.Create(serviceImplementation, serviceInterface))) {
            localServiceContainer.HandleServiceRegistered(context);
         }
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         InvokableServiceContext context;
         if (serviceContextsByService.TryGetValue(serviceImplementation, out context)) {
            if (serviceContextsByService.TryRemove(serviceImplementation, context)) {
               localServiceContainer.HandleServiceUnregistered(context);
            }
         }
      }

      public void Dispose() {
         localServiceContainer.Dispose();
      }
   }
}
