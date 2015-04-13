using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dargon.Services.Common;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public class ServiceClient : IServiceClient {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceProxyFactory serviceProxyFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IUserInvocationManager invocationManager;

      private readonly IDictionary<Type, object> serviceProxiesByInterface;
      private readonly object synchronization = new object();

      public ServiceClient(ICollectionFactory collectionFactory, IServiceProxyFactory serviceProxyFactory, IServiceContextFactory serviceContextFactory, IUserInvocationManager invocationManager) {
         this.collectionFactory = collectionFactory;
         this.serviceProxyFactory = serviceProxyFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.invocationManager = invocationManager;

         this.serviceProxiesByInterface = collectionFactory.CreateDictionary<Type, object>();
      }

      public TService GetService<TService>() where TService : class {
         lock (synchronization) {
            var serviceType = typeof(TService);
            object serviceProxy;
            if (!serviceProxiesByInterface.TryGetValue(serviceType, out serviceProxy)) {
               var serviceContext = serviceContextFactory.Create(serviceType, invocationManager);
               serviceProxy = serviceProxyFactory.CreateServiceProxy<TService>(serviceContext);
               serviceProxiesByInterface.Add(serviceType, serviceProxy);
            }
            return (TService)serviceProxy;
         }
      }

      public void Dispose() {
         this.invocationManager.Dispose();
      }
   }
}
