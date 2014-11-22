using System;
using System.Collections.Generic;
using Dargon.Services.Networking;
using Dargon.Services.Networking.Server;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Services {
   public class ServiceNode : IServiceNode {
      private readonly IConnector connector;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConcurrentDictionary<object, IServiceContext> serviceContextsByService; 

      public ServiceNode(ICollectionFactory collectionFactory, IConnector connector, IServiceContextFactory serviceContextFactory) {
         this.connector = connector;
         this.serviceContextFactory = serviceContextFactory;
         this.serviceContextsByService = collectionFactory.CreateConcurrentDictionary<object, IServiceContext>();
      }

      public void RegisterService(object service) {
         IServiceContext context = null;
         if (serviceContextsByService.TryAdd(service, () => context = serviceContextFactory.Create(service))) {
            connector.RegisterService(context);
         }
      }

      public void UnregisterService(object service) {
         IServiceContext context;
         if (serviceContextsByService.TryGetValue(service, out context)) {
            if (serviceContextsByService.TryRemove(service, context)) {
               connector.UnregisterService(context);
            }
         }
      }
   }
}
