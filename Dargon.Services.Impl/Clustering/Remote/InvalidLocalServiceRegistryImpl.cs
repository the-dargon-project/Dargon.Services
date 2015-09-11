using System;

namespace Dargon.Services.Clustering.Remote {
   public class InvalidLocalServiceRegistryImpl : LocalServiceRegistry {
      public void RegisterService(object serviceImplementation, Type serviceInterface, Guid serviceGuid) {
         ThrowOnServiceRegistryInvocation();
      }

      public void UnregisterService(Guid serviceGuid) {
         ThrowOnServiceRegistryInvocation();
      }

      private static void ThrowOnServiceRegistryInvocation() {
         throw new InvalidOperationException("Remote cluster service client does not support service registration!");
      }
   }
}
