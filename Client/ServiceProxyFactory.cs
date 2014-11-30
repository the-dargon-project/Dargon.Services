using Castle.DynamicProxy;

namespace Dargon.Services.Client {
   public class ServiceProxyFactory : IServiceProxyFactory {
      private readonly ProxyGenerator proxyGenerator;

      public ServiceProxyFactory(ProxyGenerator proxyGenerator) {
         this.proxyGenerator = proxyGenerator;
      }

      public TService CreateServiceProxy<TService>(IClientConnector clientConnector) where TService : class {
         var interceptor = new ServiceInvocationInterceptor(clientConnector);
         return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
      }
   }
}
