using Castle.DynamicProxy;
using Dargon.Services.Clustering;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;

namespace Dargon.Services.Client {
   public interface RemoteServiceProxyFactory {
      TService Create<TService>() where TService : class;
   }

   public class RemoteServiceProxyFactoryImpl : RemoteServiceProxyFactory {
      private readonly ProxyGenerator proxyGenerator;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;
      private readonly RemoteServiceInvocationValidatorFactory validatorFactory;
      private readonly ClusteringPhaseManager clusteringPhaseManager;

      public RemoteServiceProxyFactoryImpl(ProxyGenerator proxyGenerator, PortableObjectBoxConverter portableObjectBoxConverter, RemoteServiceInvocationValidatorFactory validatorFactory, ClusteringPhaseManager clusteringPhaseManager) {
         this.proxyGenerator = proxyGenerator;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
         this.validatorFactory = validatorFactory;
         this.clusteringPhaseManager = clusteringPhaseManager;
      }

      public TService Create<TService>() where TService : class {
         var serviceInterface = typeof(TService);
         var serviceGuid = AttributeUtilities.GetInterfaceGuid(serviceInterface);
         var validator = validatorFactory.Create(serviceGuid, serviceInterface);
         var translator = new InvocationResultTranslatorImpl(portableObjectBoxConverter);
         var interceptor = new RemoteServiceProxyInvocationInterceptor(serviceGuid, validator, translator, clusteringPhaseManager);
         return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
      }
   }
}
