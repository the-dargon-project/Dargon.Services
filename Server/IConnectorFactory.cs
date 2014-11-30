namespace Dargon.Services.Server {
   public interface IConnectorFactory {
      IConnector CreateServiceConnector(int port);
   }
}
