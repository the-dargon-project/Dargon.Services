using System;

namespace Dargon.Services.Server {
   public interface IConnectorWorker : IDisposable {
      void Initalize();
   }
}