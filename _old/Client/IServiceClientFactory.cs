using ItzWarty.Networking;

namespace Dargon.Services.Client {
   public interface IServiceClientFactory {
      IServiceClient Create(ITcpEndPoint endpoint);
   }
}
