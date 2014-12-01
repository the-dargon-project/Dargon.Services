using Castle.DynamicProxy;

namespace Dargon.Services.Client {
   public class ServiceProxyFactory : IServiceProxyFactory {
      private readonly ProxyGenerator proxyGenerator;

      public ServiceProxyFactory(ProxyGenerator proxyGenerator) {
         this.proxyGenerator = proxyGenerator;
      }

      public TService CreateServiceProxy<TService>(IServiceContext context) where TService : class {
         var interceptor = new ServiceInvocationInterceptor(context);
         return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
      }
   }
}
