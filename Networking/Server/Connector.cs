using System;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public class Connector : IConnector, IDisposable {
      private readonly IConnectorWorker connectorWorker;
      private readonly IConnectorContext connectorContext;
      private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName;
      private readonly object contextLock = new object();

      internal Connector(IConnectorWorker connectorWorker, IConnectorContext connectorContext, IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.connectorWorker = connectorWorker;
         this.connectorContext = connectorContext;
         this.serviceContextsByName = serviceContextsByName;
      }

      public void Initialize() {
         this.connectorContext.Initialize(serviceContextsByName);
         connectorWorker.Initalize();
      }

      public void RegisterService(IServiceContext serviceContext) {
         connectorContext.HandleServiceRegistered(serviceContext);
      }

      public void UnregisterService(IServiceContext serviceContext) {
         connectorContext.HandleServiceUnregistered(serviceContext);
      }

      public void Dispose() {
         if (connectorWorker != null) {
            connectorWorker.Dispose();
         }
      }
   }
}