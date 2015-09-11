using System.Net;

namespace Dargon.Services.Clustering {
   public interface ClusteringConfiguration {
      IPAddress Address { get; }
      int Port { get; }
      ClusteringRole ClusteringRole { get; }
   }

   public class ClusteringConfigurationFactoryImpl {
      public ClusteringConfiguration Local(IPAddress address, int port, ClusteringRole clusteringRole) {
         return new ClusteringConfigurationImpl(address, port, clusteringRole);
      }

      public ClusteringConfiguration Remote(IPEndPoint endpoint) {
         return Remote(endpoint.Address, endpoint.Port);
      }

      public ClusteringConfiguration Remote(IPAddress address, int port) {
         return new ClusteringConfigurationImpl(address, port, ClusteringRole.GuestOnly);
      }
   }

   public class ClusteringConfigurationImpl : ClusteringConfiguration {
      public ClusteringConfigurationImpl(IPAddress address, int port, ClusteringRole clusteringRole) {
         this.Address = address;
         this.Port = port;
         this.ClusteringRole = clusteringRole;
      }

      public IPAddress Address { get; private set; }
      public int Port { get; private set; }
      public ClusteringRole ClusteringRole { get; private set; }
   }
}
