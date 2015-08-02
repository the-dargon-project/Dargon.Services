namespace Dargon.Services {
   public interface IClusteringConfiguration {
      int Port { get; }
      int HeartbeatIntervalMilliseconds { get; }
      ClusteringRoleFlags ClusteringRoleFlags { get; }
   }

   public class ClusteringConfiguration : IClusteringConfiguration {
      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds) : this(port, heartbeatIntervalMilliseconds, ClusteringRoleFlags.Default) {}

      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds, ClusteringRoleFlags clusteringRoleFlags) {
         this.HeartbeatIntervalMilliseconds = heartbeatIntervalMilliseconds;
         this.Port = port;
         this.ClusteringRoleFlags = clusteringRoleFlags;
      }

      public int Port { get; private set; }
      public int HeartbeatIntervalMilliseconds { get; private set; }
      public ClusteringRoleFlags ClusteringRoleFlags { get; private set; }
   }
}
