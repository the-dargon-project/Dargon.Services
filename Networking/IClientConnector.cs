namespace Dargon.Services.Networking {
   public interface IClientConnector {
      TResult Invoke<TResult>(string service, string action, object[] arguments);
   }
}