using System;

namespace Dargon.Services.Networking {
   public interface IServiceConnectorWorker : IDisposable {
      void Start();
      void Stop();

      void SignalUpdate();
   }
}