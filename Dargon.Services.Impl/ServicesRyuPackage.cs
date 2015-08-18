using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Dargon.Ryu;
using Dargon.Services.Messaging;

namespace Dargon.Services {
   public class ServicesRyuPackage : RyuPackageV1 {
      public ServicesRyuPackage() {
         PofContext<DspPofContext>();
         Singleton<IServiceClientFactory, ServiceClientFactory>();
         Singleton<LocalServiceRegistry, LocalServiceRegistryImpl>();
         Singleton<RemoteServiceProxyContainer, RemoteServiceProxyContainerImpl>();
         Singleton<IServiceClient, ServiceClientProxyImpl>();
         Singleton<ServiceClientProxyImpl>(
            ryu => ryu.Get<IServiceClientFactory>()
                      .CreateOrJoin(ryu.Get<IClusteringConfiguration>())
         );
         Singleton<ProxyGenerator>(ryu => new ProxyGenerator(), RyuTypeFlags.IgnoreDuplicates);
      }
   }
}
