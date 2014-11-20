using System;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public class Connector : IConnector, IDisposable {
      private readonly IConnectorWorker connectorWorker;
      private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName;
      private readonly object contextLock = new object();

      internal Connector(IConnectorWorker connectorWorker, IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.connectorWorker = connectorWorker;
         this.serviceContextsByName = serviceContextsByName;
      }

      public void Initialize() {
         connectorWorker.Initalize(serviceContextsByName);
         connectorWorker.Start();
      }

      public void RegisterService(IServiceContext serviceContext) {
         lock (contextLock) {
            if (!this.serviceContextsByName.TryAdd(serviceContext.Name, serviceContext)) {
               throw new InvalidOperationException("Attempted to register service context twice!");
            } else {
               connectorWorker.SignalUpdate();
            }
         }
      }

      public void UnregisterService(IServiceContext serviceContext) {
         lock (contextLock) {
            IServiceContext removedContext;
            if (!this.serviceContextsByName.TryRemove(serviceContext.Name, out removedContext)) {
               throw new InvalidOperationException("Attempted to deregister unregistered service context!");
            } else if (removedContext != serviceContext) {
               throw new InvalidOperationException("ServiceContext of name " + serviceContext.Name + " removed, but references did not match!?");
            } else {
               connectorWorker.SignalUpdate();
            }
         }
      }

      public void Dispose() {
         if (connectorWorker != null) {
            connectorWorker.Dispose();
         }
      }
   }
}