using System;
using System.Collections.Generic;
using Dargon.Services.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public interface IServiceNodeContext : IDisposable {
      INodeConfiguration NodeConfiguration { get; }
      IConcurrentDictionary<Guid, IServiceContext> ServiceContextsByGuid { get; }
      IPhase CurrentPhase { get; }

      void Transition(IPhase phase);

      void HandleServiceRegistered(IServiceContext serviceContext);
      void HandleServiceUnregistered(IServiceContext serviceContext);
   }
}
