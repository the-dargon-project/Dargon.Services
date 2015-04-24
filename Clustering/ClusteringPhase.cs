using System;
using Dargon.Services.Server;

namespace Dargon.Services.Clustering {
   public interface ClusteringPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }
}