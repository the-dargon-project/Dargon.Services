namespace Dargon.Services.Server.Events {
   public interface IConnectorEventFactory {
      IConnectorEvent CreateServiceRegisteredEvent(IServiceContext serviceContext);
      IConnectorEvent CreateServiceUnregisteredEvent(IServiceContext serviceContext);
   }
}