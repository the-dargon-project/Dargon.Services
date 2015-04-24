namespace Dargon.Services.Client {
   public interface IServiceEndpoint {
      string Hostname { get; }
      int Port { get; }
   }
}
