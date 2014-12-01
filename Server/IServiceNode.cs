using System;

namespace Dargon.Services.Server {
   public interface IServiceNode : IDisposable {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);
   }
}
