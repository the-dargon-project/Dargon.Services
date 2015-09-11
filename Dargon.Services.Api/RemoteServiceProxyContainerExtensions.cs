using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services {
   public static class RemoteServiceProxyContainerExtensions {
      public static TService GetService<TService>(this RemoteServiceProxyContainer remoteServiceProxyContainer) where TService : class {
         var serviceInterface = typeof(TService);
         Guid interfaceGuid;
         if (!AttributeUtilitiesInternal.TryGetInterfaceGuid(serviceInterface, out interfaceGuid)) {
            throw new ArgumentException($"Service Interface {serviceInterface.FullName} does not expose Guid Attribute!");
         } else {
            return remoteServiceProxyContainer.GetService<TService>(interfaceGuid);
         }
      }
   }
}
