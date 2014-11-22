using System;
using System.Diagnostics;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public class ConnectorContext : IContext {
      private IPhase phase;
      private IConcurrentDictionary<string, IServiceContext> serviceContextsByName;
      private bool disposed = false;

      public ConnectorContext(IPhase initialPhase) {
         this.phase = initialPhase;
      }

      public IConcurrentDictionary<string, IServiceContext> ServiceContextsByName { get { return serviceContextsByName; } }
      public IPhase CurrentPhase { get { return phase; } }

      public void Initialize(IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.serviceContextsByName = serviceContextsByName;
      }

      public void Transition(IPhase phase) {
         ThrowIfDisposed();

         this.phase = phase;
      }

      public void HandleUpdate() {
         ThrowIfDisposed();

         this.phase.HandleUpdate();
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            this.phase.Dispose();
         }
      }

      internal void ThrowIfDisposed() {
         if (disposed) {
            throw new ObjectDisposedException("this", "This connector context has already been disposed!");
         }
      }
   }
}