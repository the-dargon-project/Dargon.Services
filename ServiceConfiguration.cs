namespace Dargon.Services {
   public class ServiceConfiguration : IServiceConfiguration {
      private readonly int port;
      private readonly int heartbeatIntervalMilliseconds;

      public ServiceConfiguration(int port, int heartbeatIntervalMilliseconds) {
         this.heartbeatIntervalMilliseconds = heartbeatIntervalMilliseconds;
         this.port = port;
      }

      public int Port { get { return port; } }
      public int HeartbeatIntervalMilliseconds { get { return heartbeatIntervalMilliseconds; } }
   }
}