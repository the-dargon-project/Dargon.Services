namespace Dargon.Services.Networking.Events {
   public class ConnectorEvent : IConnectorEvent {
      private readonly ConnectorEventType type;
      private readonly IServiceContext serviceContext;
      private readonly object payload;

      public ConnectorEvent(ConnectorEventType type, IServiceContext serviceContext, object payload) {
         this.payload = payload;
         this.serviceContext = serviceContext;
         this.type = type;
      }

      public ConnectorEventType Type { get { return type; } }
      public IServiceContext ServiceContext { get { return serviceContext; } }
      public object Payload { get { return payload; } }
   }
}