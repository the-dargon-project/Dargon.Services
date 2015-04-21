using Dargon.Services.Server;

namespace Dargon.Services {
   public interface INodeConfiguration {
      int Port { get; }
      int HeartbeatIntervalMilliseconds { get; }
      NodeOwnershipFlags NodeOwnershipFlags { get; }
   }
}
