using Dargon.Services.Networking.Server;

namespace Dargon.Services.Networking {
   public interface IConnectorFactory {
      IClientConnector CreateClientConnector(IServiceEndpoint endpoint);
      IConnector CreateServiceConnector(int port);
   }
}
