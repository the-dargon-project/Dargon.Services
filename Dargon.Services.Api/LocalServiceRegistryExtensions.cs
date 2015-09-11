using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dargon.Services {
   public static class LocalServiceRegistryExtensions {
      public static void RegisterService(this LocalServiceRegistry localServiceRegistry, object serviceImplementation, Type serviceInterface) {
         Guid interfaceGuid;
         if (!AttributeUtilitiesInternal.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            localServiceRegistry.RegisterService(serviceImplementation, serviceInterface, interfaceGuid);
         }
      }

      public static void UnregisterService(this LocalServiceRegistry localServiceRegistry, Type serviceInterface) {
         Guid interfaceGuid;
         if (!AttributeUtilitiesInternal.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            localServiceRegistry.UnregisterService(interfaceGuid);
         }
      }
   }
}
