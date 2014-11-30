using System;
using System.Collections.Generic;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public class ServiceClient : IServiceClient {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceProxyFactory serviceProxyFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IClientConnector clientConnector;

      private readonly IDictionary<Type, object> serviceProxiesByInterface;
      private readonly object synchronization = new object();

      public ServiceClient(ICollectionFactory collectionFactory, IServiceProxyFactory serviceProxyFactory, IServiceContextFactory serviceContextFactory, IClientConnector clientConnector) {
         this.collectionFactory = collectionFactory;
         this.serviceProxyFactory = serviceProxyFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.clientConnector = clientConnector;

         this.serviceProxiesByInterface = collectionFactory.CreateDictionary<Type, object>();
      }

      public TService GetService<TService>() where TService : class {
         lock (synchronization) {
            var serviceType = typeof(TService);
            object serviceProxy;
            if (!serviceProxiesByInterface.TryGetValue(serviceType, out serviceProxy)) {
               var serviceContext = serviceContextFactory.Create(clientConnector);
               serviceProxy = serviceProxyFactory.CreateServiceProxy<TService>(serviceContext);
               serviceProxiesByInterface.Add(serviceType, serviceProxy);
            }
            return (TService)serviceProxy;
         }
      }
   }

   public interface IServiceContextFactory {
      IServiceContext Create(IClientConnector clientConnector);
   }

   public interface IServiceContext {

   }

   public class ServiceContext : IServiceContext {
   }
}
