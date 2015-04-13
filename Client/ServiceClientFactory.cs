using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.Common;
using ItzWarty.Collections;
using ItzWarty.Networking;

namespace Dargon.Services.Client {
   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceProxyFactory serviceProxyFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IInvocationManagerFactory invocationManagerFactory;

      public ServiceClientFactory(ICollectionFactory collectionFactory, IServiceProxyFactory serviceProxyFactory, IServiceContextFactory serviceContextFactory, IInvocationManagerFactory invocationManagerFactory) {
         this.collectionFactory = collectionFactory;
         this.serviceProxyFactory = serviceProxyFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.invocationManagerFactory = invocationManagerFactory;
      }

      public IServiceClient Create(ITcpEndPoint endpoint) {
         var connector = invocationManagerFactory.Create(endpoint);
         return new ServiceClient(collectionFactory, serviceProxyFactory, serviceContextFactory, connector);
      }
   }
}
