using System;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public class Connector : IConnector {
      private readonly IConnectorContext connectorContext;
      private readonly object contextLock = new object();

      internal Connector(IConnectorContext connectorContext) {
         this.connectorContext = connectorContext;
      }

      public void RegisterService(IServiceContext serviceContext) {
         connectorContext.HandleServiceRegistered(serviceContext);
      }

      public void UnregisterService(IServiceContext serviceContext) {
         connectorContext.HandleServiceUnregistered(serviceContext);
      }

      public void Dispose() {
         connectorContext.Dispose();
      }
   }
}