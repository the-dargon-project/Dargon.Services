using System;

namespace Dargon.Services {
   public class ServiceClientProxyImpl : ServiceClient {
      private readonly LocalServiceRegistry localServiceRegistry;
      private readonly RemoteServiceProxyContainer remoteServiceProxyContainer;

      public ServiceClientProxyImpl(LocalServiceRegistry localServiceRegistry, RemoteServiceProxyContainer remoteServiceProxyContainer) {
         this.localServiceRegistry = localServiceRegistry;
         this.remoteServiceProxyContainer = remoteServiceProxyContainer;
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface, Guid serviceGuid) {
         localServiceRegistry.RegisterService(serviceImplementation, serviceInterface, serviceGuid);
      }

      public void UnregisterService(Guid serviceGuid) {
         localServiceRegistry.UnregisterService(serviceGuid);
      }

      public TService GetService<TService>(Guid serviceGuid) where TService : class {
         return remoteServiceProxyContainer.GetService<TService>(serviceGuid);
      }
   }
}