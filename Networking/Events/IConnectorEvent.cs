namespace Dargon.Services.Networking.Events {
   public interface IConnectorEvent {
      ConnectorEventType Type { get; }
      IServiceContext ServiceContext { get; }
      object Payload { get; }
   }
}
