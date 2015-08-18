using System;

namespace Dargon.Services {
   public interface IServiceClient : LocalServiceRegistry, RemoteServiceProxyContainer {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);
   }
}
