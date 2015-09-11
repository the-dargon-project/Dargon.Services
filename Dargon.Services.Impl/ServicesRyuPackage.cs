using Castle.DynamicProxy;
using Dargon.Ryu;
using Dargon.Services.Clustering;
using Dargon.Services.Clustering.Local;
using Dargon.Services.Messaging;

namespace Dargon.Services {
   public class ServicesRyuPackage : RyuPackageV1 {
      public ServicesRyuPackage() {
         PofContext<DspPofContext>();
         Singleton<ServiceClientFactory, ServiceClientFactoryImpl>();
         Singleton<LocalServiceRegistry, LocalServiceRegistryImpl>();
         Singleton<RemoteServiceProxyContainer, RemoteServiceProxyContainerImpl>();
         Singleton<ServiceClient, ServiceClientProxyImpl>();
         Singleton<ServiceClientProxyImpl>(
            ryu => ryu.Get<ServiceClientFactoryImpl>()
                      .Construct(ryu.Get<ClusteringConfiguration>())
         );
         Singleton<ProxyGenerator>(ryu => new ProxyGenerator(), RyuTypeFlags.IgnoreDuplicates);
      }
   }
}
