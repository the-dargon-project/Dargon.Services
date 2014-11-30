namespace Dargon.Services.Server {
   public interface IServiceNodeFactory {
      IServiceNode CreateOrJoin(int port);
   }
}
