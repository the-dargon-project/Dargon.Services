using System;
using Dargon.Services.Server.Events;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public class ConnectorConnectorContext : IConnectorContext {
      private readonly IConnectorEventFactory connectorEventFactory;
      private readonly IConcurrentQueue<IConnectorEvent> connectorEventQueue;
      private IPhase phase;
      private IConcurrentDictionary<Guid, IServiceContext> serviceContextsByGuid;
      private bool disposed = false;

      public ConnectorConnectorContext(IConnectorEventFactory connectorEventFactory, ICollectionFactory collectionFactory, IPhase initialPhase) {
         this.connectorEventFactory = connectorEventFactory;
         this.connectorEventQueue = collectionFactory.CreateConcurrentQueue<IConnectorEvent>();
         this.phase = initialPhase;
      }

      public IConcurrentDictionary<Guid, IServiceContext> ServiceContextsByGuid { get { return serviceContextsByGuid; } }
      public IPhase CurrentPhase { get { return phase; } }
      public IConcurrentQueue<IConnectorEvent> EventQueue { get { return connectorEventQueue; } }

      public void Initialize(IConcurrentDictionary<Guid, IServiceContext> serviceContextsByGuid) {
         this.serviceContextsByGuid = serviceContextsByGuid;
      }

      public void Transition(IPhase phase) {
         ThrowIfDisposed();

         this.phase = phase;
      }

      public void HandleServiceRegistered(IServiceContext serviceContext) {
         var @event = connectorEventFactory.CreateServiceRegisteredEvent(serviceContext);
         connectorEventQueue.Enqueue(@event);
      }

      public void HandleServiceUnregistered(IServiceContext serviceContext) {
         var @event = connectorEventFactory.CreateServiceUnregisteredEvent(serviceContext);
         connectorEventQueue.Enqueue(@event);
      }

      public void RunIteration() {
         ThrowIfDisposed();

         this.phase.RunIteration();
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            this.phase.Dispose();
         }
      }

      internal void ThrowIfDisposed() {
         if (disposed) {
            const string error = "This connector context has already been disposed!";
            throw new ObjectDisposedException("this", error);
         }
      }
   }
}