namespace Dargon.Services.Server {
   public interface IServiceContextFactory {
      IServiceContext Create(object service);
   }
}