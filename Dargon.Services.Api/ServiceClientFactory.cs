using System.Net;

namespace Dargon.Services {
   public interface ServiceClientFactory {
      ServiceClient Local(int port);
      ServiceClient Local(int port, ClusteringRole clusteringRole);

      ServiceClient Remote(IPEndPoint endpoint);
      ServiceClient Remote(IPAddress address, int port);
   }
}
