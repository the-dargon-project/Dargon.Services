using System;

namespace Dargon.Services {
   public interface IServiceClient : IDisposable {
      void RegisterService(object serviceImplementation, Type serviceInterface);
      void UnregisterService(object serviceImplementation, Type serviceInterface);

      TService GetService<TService>() where TService : class;
   }
}
