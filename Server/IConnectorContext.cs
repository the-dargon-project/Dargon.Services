﻿using System;
using Dargon.Services.Server.Events;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public interface IConnectorContext : IPhase {
      IConcurrentDictionary<Guid, IServiceContext> ServiceContextsByGuid { get; }
      IPhase CurrentPhase { get; }
      IConcurrentQueue<IConnectorEvent> EventQueue { get; }

      void Initialize(IConcurrentDictionary<Guid, IServiceContext> serviceContextsByName);
      void Transition(IPhase phase);

      void HandleServiceRegistered(IServiceContext serviceContext);
      void HandleServiceUnregistered(IServiceContext serviceContext);
   }
}