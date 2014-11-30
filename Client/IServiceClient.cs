namespace Dargon.Services.Client {
   public interface IServiceClient {
      TService GetService<TService>() where TService : class;
   }
}
