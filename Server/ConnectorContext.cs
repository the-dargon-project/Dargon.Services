using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Services.Server {
   public class ConnectorContext : IConnectorContext {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceConfiguration serviceConfiguration;
      private readonly IConcurrentDictionary<Guid, IServiceContext> serviceContextsByGuid;
      private readonly object synchronization = new object();
      private IPhase phase;
      private bool disposed = false;

      public ConnectorContext(ICollectionFactory collectionFactory, IServiceConfiguration serviceConfiguration) {
         this.collectionFactory = collectionFactory;
         this.serviceConfiguration = serviceConfiguration;
         this.serviceContextsByGuid = collectionFactory.CreateConcurrentDictionary<Guid, IServiceContext>();
      }

      public IServiceConfiguration ServiceConfiguration { get { return serviceConfiguration; } }
      public IConcurrentDictionary<Guid, IServiceContext> ServiceContextsByGuid { get { return serviceContextsByGuid; } }
      public IPhase CurrentPhase { get { return phase; } }

      public void Transition(IPhase phase) {
         lock (synchronization) {
            ThrowIfDisposed();

            Debug.WriteLine("Transition from phase " + (this.phase == null ? "null" : this.phase.ToString()) + " to " + (phase == null ? "null" : phase.ToString()));
            this.phase = phase;
            this.phase.HandleEnter();
         }
      }

      public void HandleServiceRegistered(IServiceContext serviceContext) {
         lock (synchronization) {
            ServiceContextsByGuid.Add(serviceContext.Guid, serviceContext);
            phase.HandleServiceRegistered(serviceContext);
         }
      }

      public void HandleServiceUnregistered(IServiceContext serviceContext) {
         lock (synchronization) {
            ServiceContextsByGuid.Remove(new KeyValuePair<Guid, IServiceContext>(serviceContext.Guid, serviceContext));
            phase.HandleServiceUnregistered(serviceContext);
         }
      }

      public void Dispose() {
         lock (synchronization) {
            if (!disposed) {
               disposed = true;
               this.phase.Dispose();
            }
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