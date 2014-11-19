using System.Collections.Generic;
using Dargon.Services.Networking;

namespace Dargon.Services {
   public class ServiceNode : IServiceNode {
      private readonly IServiceConnector serviceConnector;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly Dictionary<object, IServiceContext> serviceContextsByService = new Dictionary<object, IServiceContext>(); 

      public ServiceNode(IServiceConnector serviceConnector, IServiceContextFactory serviceContextFactory) {
         this.serviceConnector = serviceConnector;
         this.serviceContextFactory = serviceContextFactory;
      }

      public void RegisterService(object service) {
         if (!serviceContextsByService.ContainsKey(service)) {
            var context = serviceContextFactory.Create(service);
            serviceContextsByService.Add(service, context);
            serviceConnector.RegisterService(context);
         }
      }

      public void UnregisterService(object service) {
         IServiceContext context;
         if (serviceContextsByService.TryGetValue(service, out context)) {
            serviceContextsByService.Remove(service);
            serviceConnector.UnregisterService(context);
         }
      }
   }
}
