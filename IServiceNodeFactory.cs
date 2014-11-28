namespace Dargon.Services {
   public interface IServiceNodeFactory {
      IServiceNode CreateOrJoin(int port);
   }
}
