using System.Diagnostics;
using System.Threading;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using Dargon.Services.PortableObjects;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ServiceClientFT : NMockitoInstance {
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly ServiceClientFactory serviceClientFactory;
      private readonly ISocketFactory socketFactory;
      private const int kTestPort = 20001;
      private const string kVersioningServiceVersion = "123.343.5-asdf";

      public ServiceClientFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(proxyGenerator);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         IInvocationStateFactory invocationStateFactory = new InvocationStateFactory(threadingProxy);
         IPofContext pofContext = new TestPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, socketFactory, invocationStateFactory, pofSerializer);
         serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, serviceContextFactory, connectorFactory);
      }

      [Fact]
      public void Run() {
         new Thread(() => {
            var listener = new SocketFactory()
         }).Start();

         var localEndpoint = tcpEndPointFactory.CreateLoopbackEndPoint(kTestPort);
         var client = serviceClientFactory.Create(localEndpoint);
         var versioningService = client.GetService<IVersioningService>();
         var version = versioningService.GetVersion();
         AssertEquals(kVersioningServiceVersion, version);
      }

      public class TestPofContext : PofContext {
         public TestPofContext() {
            this.MergeContext(new DspPofContext());
         }
      }

      public interface IVersioningService {
         string GetVersion();
      }
   }
}
