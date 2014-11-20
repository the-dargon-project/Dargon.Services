namespace Dargon.Services
{
   public interface IServiceEndpoint {
      string Hostname { get; }
      int Port { get; }
   }
}