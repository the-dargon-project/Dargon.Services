namespace Dargon.Services.Server {
   public interface IConnector {
      void RegisterService(IServiceContext context);
      void UnregisterService(IServiceContext serviceContext);
   }
}
