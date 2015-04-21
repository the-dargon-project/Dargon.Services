using System;
using Dargon.Services.Server;

namespace Dargon.Services.Phases {
   public interface IPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(IServiceContext serviceContext);
      void HandleServiceUnregistered(IServiceContext serviceContext);
   }
}