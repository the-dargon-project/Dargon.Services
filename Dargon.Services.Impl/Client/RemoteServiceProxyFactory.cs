using System;
using Castle.DynamicProxy;
using Dargon.Services.Clustering;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;

namespace Dargon.Services.Client {
   public interface RemoteServiceProxyFactory {
      TService Create<TService>() where TService : class;
      TService Create<TService>(Guid guid) where TService : class;
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
         Guid serviceGuid;
         if (!AttributeUtilities.TryGetInterfaceGuid(serviceInterface, out serviceGuid)) {
            throw new InvalidOperationException($"Remote service interface {serviceInterface.FullName} lacks service guid attribute.");
         } else {
            return Create<TService>(serviceGuid);
         }
      }

      public TService Create<TService>(Guid serviceGuid) where TService : class {
         var serviceInterface = typeof(TService);
         var validator = validatorFactory.Create(serviceGuid, serviceInterface);
         var translator = new InvocationResultTranslatorImpl(portableObjectBoxConverter);
         var interceptor = new RemoteServiceProxyInvocationInterceptor(serviceGuid, validator, translator, clusteringPhaseManager);
         return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
      }
   }
}
