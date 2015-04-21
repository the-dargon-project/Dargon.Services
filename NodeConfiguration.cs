namespace Dargon.Services {
   public class NodeConfiguration : INodeConfiguration {
      private readonly int port;
      private readonly int heartbeatIntervalMilliseconds;
      private readonly NodeOwnershipFlags nodeOwnershipFlags;

      public NodeConfiguration(int port, int heartbeatIntervalMilliseconds) : this(port, heartbeatIntervalMilliseconds, NodeOwnershipFlags.Default){ }

      public NodeConfiguration(int port, int heartbeatIntervalMilliseconds, NodeOwnershipFlags nodeOwnershipFlags) {
         this.heartbeatIntervalMilliseconds = heartbeatIntervalMilliseconds;
         this.port = port;
         this.nodeOwnershipFlags = nodeOwnershipFlags;
      }

      public int Port { get { return port; } }
      public int HeartbeatIntervalMilliseconds { get { return heartbeatIntervalMilliseconds; } }
      public NodeOwnershipFlags NodeOwnershipFlags { get { return nodeOwnershipFlags; } }
   }
}