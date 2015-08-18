using System;
using Dargon.Services.Client;
using ItzWarty.Collections;

namespace Dargon.Services {
   public class RemoteServiceProxyContainerImpl : RemoteServiceProxyContainer {
      private readonly RemoteServiceProxyFactory remoteServiceProxyFactory;
      private readonly IConcurrentDictionary<Type, object> serviceProxiesByInterface;

      public RemoteServiceProxyContainerImpl(RemoteServiceProxyFactory remoteServiceProxyFactory, IConcurrentDictionary<Type, object> serviceProxiesByInterface) {
         this.remoteServiceProxyFactory = remoteServiceProxyFactory;
         this.serviceProxiesByInterface = serviceProxiesByInterface;
      }

      public TService GetService<TService>() where TService : class {
         return (TService)serviceProxiesByInterface.GetOrAdd(
            typeof(TService),
            add => remoteServiceProxyFactory.Create<TService>()
            );
      }
   }
}