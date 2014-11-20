using System.Collections.Generic;
using Dargon.Services.Networking;
using Dargon.Services.Networking.Server;

namespace Dargon.Services {
   public class ServiceNode : IServiceNode {
      private readonly IConnector connector;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly Dictionary<object, IServiceContext> serviceContextsByService = new Dictionary<object, IServiceContext>(); 

      public ServiceNode(IConnector connector, IServiceContextFactory serviceContextFactory) {
         this.connector = connector;
         this.serviceContextFactory = serviceContextFactory;
      }

      public void RegisterService(object service) {
         if (!serviceContextsByService.ContainsKey(service)) {
            var context = serviceContextFactory.Create(service);
            serviceContextsByService.Add(service, context);
            connector.RegisterService(context);
         }
      }

      public void UnregisterService(object service) {
         IServiceContext context;
         if (serviceContextsByService.TryGetValue(service, out context)) {
            serviceContextsByService.Remove(service);
            connector.UnregisterService(context);
         }
      }
   }
}
