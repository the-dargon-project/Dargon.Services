using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.Collections;
using ItzWarty.Networking;

namespace Dargon.Services.Client {
   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceProxyFactory serviceProxyFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConnectorFactory connectorFactory;

      public ServiceClientFactory(ICollectionFactory collectionFactory, IServiceProxyFactory serviceProxyFactory, IServiceContextFactory serviceContextFactory, IConnectorFactory connectorFactory) {
         this.collectionFactory = collectionFactory;
         this.serviceProxyFactory = serviceProxyFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.connectorFactory = connectorFactory;
      }

      public IServiceClient Create(ITcpEndPoint endpoint) {
         var connector = connectorFactory.Create(endpoint);
         return new ServiceClient(collectionFactory, serviceProxyFactory, serviceContextFactory, connector);
      }
   }
}
