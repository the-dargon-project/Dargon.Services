namespace Dargon.Services.Client {
   public interface IServiceProxyFactory {
      TService CreateServiceProxy<TService>(IServiceContext serviceContext) where TService : class;
   }
}