namespace Dargon.Services.Server {
   public interface IServiceNode {
      void RegisterService(object service);
      void UnregisterService(object service);
   }
}
