namespace Dargon.Services {
   public interface IServiceNode {
      void RegisterService(object service);
      void UnregisterService(object service);
   }
}
