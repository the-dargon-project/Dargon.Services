using System;

namespace Dargon.Services {
   public interface LocalServiceRegistry {
      void RegisterService(object serviceImplementation, Type serviceInterface, Guid serviceGuid);
      void UnregisterService(Guid serviceGuid);
   }
}