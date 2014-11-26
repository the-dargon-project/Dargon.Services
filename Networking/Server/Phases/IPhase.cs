using System;

namespace Dargon.Services.Networking.Server.Phases {
   public interface IPhase : IDisposable {
      void RunIteration();
   }
}