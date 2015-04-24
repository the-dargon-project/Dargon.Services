using Dargon.Services.Server;

namespace Dargon.Services {
   public interface IClusteringConfiguration {
      int Port { get; }
      int HeartbeatIntervalMilliseconds { get; }
      ClusteringRoleFlags ClusteringRoleFlags { get; }
   }
   public class ClusteringConfiguration : IClusteringConfiguration {
      private readonly int port;
      private readonly int heartbeatIntervalMilliseconds;
      private readonly ClusteringRoleFlags clusteringRoleFlags;

      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds) : this(port, heartbeatIntervalMilliseconds, ClusteringRoleFlags.Default) { }

      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds, ClusteringRoleFlags clusteringRoleFlags) {
         this.heartbeatIntervalMilliseconds = heartbeatIntervalMilliseconds;
         this.port = port;
         this.clusteringRoleFlags = clusteringRoleFlags;
      }

      public int Port { get { return port; } }
      public int HeartbeatIntervalMilliseconds { get { return heartbeatIntervalMilliseconds; } }
      public ClusteringRoleFlags ClusteringRoleFlags { get { return clusteringRoleFlags; } }
   }
}
