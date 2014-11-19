namespace Dargon.Services {
   public interface IServiceContextFactory {
      IServiceContext Create(object service);
   }
}