using System;

namespace Dargon.Services.Server {
   public interface IConnector : IDisposable {
      void RegisterService(IServiceContext context);
      void UnregisterService(IServiceContext serviceContext);
   }
}
