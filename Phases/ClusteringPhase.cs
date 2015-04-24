using Dargon.Services.Server;
using System;

namespace Dargon.Services.Phases {
   public interface ClusteringPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }
}