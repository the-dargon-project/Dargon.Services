using System;
using Castle.DynamicProxy;
using Dargon.Services.Utilities;
using ItzWarty.Collections;

namespace Dargon.Services.Clustering.Remote {
   public class LoadBalancedRemoteServiceProxyContainerImpl : RemoteServiceProxyContainer {
      private readonly IConcurrentDictionary<Guid, object> serviceProxiesByGuid = new ConcurrentDictionary<Guid, object>();
      private readonly ProxyGenerator proxyGenerator;
      private readonly RemoteServiceClientContainer remoteServiceClientContainer;

      public LoadBalancedRemoteServiceProxyContainerImpl(ProxyGenerator proxyGenerator, RemoteServiceClientContainer remoteServiceClientContainer) {
         this.proxyGenerator = proxyGenerator;
         this.remoteServiceClientContainer = remoteServiceClientContainer;
      }

      public TService GetService<TService>() where TService : class {
         Guid serviceGuid;
         var serviceInterface = typeof(TService);
         if (!AttributeUtilities.TryGetInterfaceGuid(serviceInterface, out serviceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            return GetService<TService>(serviceGuid);
         }
      }

      public TService GetService<TService>(Guid serviceGuid) where TService : class {
         return (TService)serviceProxiesByGuid.GetOrAdd(serviceGuid, ConstructServiceProxy<TService>);
      }

      private TService ConstructServiceProxy<TService>(Guid serviceGuid) where TService : class {
         var interceptor = new LoadBalancedServiceProxyInterceptorImpl<TService>(remoteServiceClientContainer, serviceGuid);
         return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
      }
   }
}