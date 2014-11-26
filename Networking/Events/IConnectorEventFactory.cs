namespace Dargon.Services.Networking.Events {
   public interface IConnectorEventFactory {
      IConnectorEvent CreateServiceRegisteredEvent(IServiceContext serviceContext);
      IConnectorEvent CreateServiceUnregisteredEvent(IServiceContext serviceContext);
   }
}