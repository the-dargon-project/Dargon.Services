using System;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public interface IConnectorWorker : IDisposable {
      void Initalize();
   }
}