using System;
using Dargon.Services.Server;

namespace Dargon.Services.Phases {
   public interface IPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }
}