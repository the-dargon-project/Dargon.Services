using System;

namespace Dargon.Services.Server.Phases {
   public interface IPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(IServiceContext serviceContext);
      void HandleServiceUnregistered(IServiceContext serviceContext);
   }
}