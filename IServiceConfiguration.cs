namespace Dargon.Services {
   public interface IServiceConfiguration {
      int Port { get; }
      int HeartbeatIntervalMilliseconds { get; }
   }
}
