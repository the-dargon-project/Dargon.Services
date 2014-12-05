namespace Dargon.Services.Server {
   public interface IServiceConfiguration {
      int Port { get; }
      int HeartbeatIntervalMilliseconds { get; }
      NodeOwnershipFlags NodeOwnershipFlags { get; }
   }
}
