using System;

namespace Dargon.Services.Server.Phases {
   public interface IPhase : IDisposable {
      void RunIteration();
   }
}