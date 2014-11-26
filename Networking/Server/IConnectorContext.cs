using System;
using Dargon.Services.Networking.Events;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public interface IConnectorContext : IPhase {
      IConcurrentDictionary<Guid, IServiceContext> ServiceContextsByGuid { get; }
      IPhase CurrentPhase { get; }
      IConcurrentQueue<IConnectorEvent> EventQueue { get; }

      void Initialize(IConcurrentDictionary<string, IServiceContext> serviceContextsByName);
      void Transition(IPhase phase);

      void HandleServiceRegistered(IServiceContext serviceContext);
      void HandleServiceUnregistered(IServiceContext serviceContext);
   }
}
