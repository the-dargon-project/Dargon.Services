using System;
using System.Runtime.InteropServices;
using Dargon.Services.Client;
using Dargon.Services.Utilities;
using Extensions = ItzWarty.Extensions;

namespace Dargon.Services {
   public class ServiceClientProxyImpl : IServiceClient {
      private readonly LocalServiceRegistry localServiceRegistry;
      private readonly RemoteServiceProxyContainer remoteServiceProxyContainer;

      public ServiceClientProxyImpl(LocalServiceRegistry localServiceRegistry, RemoteServiceProxyContainer remoteServiceProxyContainer) {
         this.localServiceRegistry = localServiceRegistry;
         this.remoteServiceProxyContainer = remoteServiceProxyContainer;
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface) {
         Guid interfaceGuid;
         if (!AttributeUtilities.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            localServiceRegistry.RegisterService(serviceImplementation, serviceInterface, interfaceGuid);
         }
      }

      public void RegisterService(object serviceImplementation, Type serviceInterface, Guid serviceGuid) {
         localServiceRegistry.RegisterService(serviceImplementation, serviceInterface, serviceGuid);
      }

      public void UnregisterService(object serviceImplementation, Type serviceInterface) {
         Guid interfaceGuid;
         if (!AttributeUtilities.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            localServiceRegistry.UnregisterService(serviceImplementation, interfaceGuid);
         }
      }

      public void UnregisterService(object serviceImplementation, Guid serviceGuid) {
         localServiceRegistry.UnregisterService(serviceImplementation, serviceGuid);
      }

      public TService GetService<TService>() where TService : class {
         return remoteServiceProxyContainer.GetService<TService>();
      }

      public TService GetService<TService>(Guid serviceGuid) where TService : class {
         return remoteServiceProxyContainer.GetService<TService>(serviceGuid);
      }
   }
}