using System;
using Dargon.Services.Client;
using Dargon.Services.Utilities;
using ItzWarty.Collections;

namespace Dargon.Services.Clustering.Local {
   public class RemoteServiceProxyContainerImpl : RemoteServiceProxyContainer {
      private readonly RemoteServiceProxyFactory remoteServiceProxyFactory;
      private readonly IConcurrentDictionary<Type, object> serviceProxiesByInterface;

      public RemoteServiceProxyContainerImpl(RemoteServiceProxyFactory remoteServiceProxyFactory, IConcurrentDictionary<Type, object> serviceProxiesByInterface) {
         this.remoteServiceProxyFactory = remoteServiceProxyFactory;
         this.serviceProxiesByInterface = serviceProxiesByInterface;
      }

      public TService GetService<TService>() where TService : class {
         Type serviceInterface = typeof(TService);
         Guid interfaceGuid;
         if (!AttributeUtilities.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            return GetService<TService>(interfaceGuid);
         }
      }

      public TService GetService<TService>(Guid serviceGuid) where TService : class {
         return (TService)serviceProxiesByInterface.GetOrAdd(
            typeof(TService),
            add => remoteServiceProxyFactory.Create<TService>(serviceGuid)
         );
      }
   }
}