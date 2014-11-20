namespace Dargon.Services.Networking.Server {
   public interface IConnector {
      void RegisterService(IServiceContext context);
      void UnregisterService(IServiceContext serviceContext);
   }
}
