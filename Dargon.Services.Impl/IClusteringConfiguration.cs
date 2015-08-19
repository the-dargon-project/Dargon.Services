using System;
using System.Net;

namespace Dargon.Services {
   public interface IClusteringConfiguration {
      IPAddress RemoteAddress { get; }
      int Port { get; }
      ClusteringRoleFlags ClusteringRoleFlags { get; }

      [Obsolete("This property was never used.")]
      int HeartbeatIntervalMilliseconds { get; }
   }

   public class ClusteringConfiguration : IClusteringConfiguration {
      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds) : this(port, heartbeatIntervalMilliseconds, ClusteringRoleFlags.Default) {}

      public ClusteringConfiguration(int port, int heartbeatIntervalMilliseconds, ClusteringRoleFlags clusteringRoleFlags)
         : this(IPAddress.Loopback, port, clusteringRoleFlags) { }

      public ClusteringConfiguration(IPAddress remoteAddress, int port, ClusteringRoleFlags clusteringRoleFlags) {
         this.RemoteAddress = remoteAddress;
         this.Port = port;
         this.ClusteringRoleFlags = clusteringRoleFlags;
      }

      public IPAddress RemoteAddress { get; private set; }
      public int Port { get; private set; }
      public ClusteringRoleFlags ClusteringRoleFlags { get; private set; }

      [Obsolete("This property was never used.")]
      public int HeartbeatIntervalMilliseconds => -1;
   }
}
