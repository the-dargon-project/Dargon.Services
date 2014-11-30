namespace Dargon.Services.Server.Events {
   public class ConnectorEventFactory : IConnectorEventFactory {
      public IConnectorEvent CreateServiceRegisteredEvent(IServiceContext serviceContext) {
         return new ConnectorEvent(ConnectorEventType.ServiceRegistered, serviceContext, null);
      }

      public IConnectorEvent CreateServiceUnregisteredEvent(IServiceContext serviceContext) {
         return new ConnectorEvent(ConnectorEventType.ServiceUnregistered, serviceContext, null);
      }
   }
}