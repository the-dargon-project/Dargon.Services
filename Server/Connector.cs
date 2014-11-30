using System;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public class Connector : IConnector, IDisposable {
      private readonly IConnectorWorker connectorWorker;
      private readonly IConnectorContext connectorContext;
      private readonly IConcurrentDictionary<Guid, IServiceContext> serviceContextsByGuid;
      private readonly object contextLock = new object();

      internal Connector(IConnectorWorker connectorWorker, IConnectorContext connectorContext, IConcurrentDictionary<Guid, IServiceContext> serviceContextsByGuid) {
         this.connectorWorker = connectorWorker;
         this.connectorContext = connectorContext;
         this.serviceContextsByGuid = serviceContextsByGuid;
      }

      public void Initialize() {
         this.connectorContext.Initialize(serviceContextsByGuid);
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