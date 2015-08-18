namespace Dargon.Services {
   public interface RemoteServiceProxyContainer {
      TService GetService<TService>() where TService : class;
   }
}