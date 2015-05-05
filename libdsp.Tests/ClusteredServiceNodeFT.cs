using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;

namespace Dargon.Services {
   public class ClusteredServiceNodeFT : NMockitoInstance {
      private readonly IServiceClientFactory serviceClientFactory;

      private const int kTestPort = 20001;
      private const int kHeartBeatIntervalMilliseconds = 30000;
      private readonly IClusteringConfiguration clusteringConfiguration = new ClusteringConfiguration(kTestPort, kHeartBeatIntervalMilliseconds);

      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";
      private const string kLoginServiceGuid = "E5C6A3A0-958A-48F6-9875-DF0C4FA561C1";
      private const string kLoginServiceStatus = "Okay";
      private const string kQueueServiceGuid = "69EE0CAF-B105-492C-9DF2-851F6207264C";
      private const string kQueueServiceWaitTimeMillis = "12345";
      private const string kShopServiceGuid = "EFBE6150-CC18-441C-9757-23C41823F8C3";
      private const string kShopServiceStatus = "Okay";

      public ClusteredServiceNodeFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new DspPofContext();
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory);
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory);
         serviceClientFactory = new ServiceClientFactory(proxyGenerator, collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, invokableServiceContextFactory);
      }

      [Fact]
      public void Run() {
         Action<string> log = x => Debug.WriteLine("T: " + x);
         log("Spawning Service Node 1.");
         var serviceNode1 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode1.RegisterService(new VersioningService(), typeof(IVersioningService));

         log("Spawning Service Node 2.");
         var serviceNode2 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode2.RegisterService(new LoginService(), typeof(ILoginService));
         serviceNode2.RegisterService(new ShopService(), typeof(IShopService));

         log("Spawning Service Node 3.");
         var serviceNode3 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode3.RegisterService(new QueueService(), typeof(IQueueService));

         // Give 500ms for nodes to discover services.
         Thread.Sleep(500);

         log("Using remote service proxy of host node:");
         RunHostClientLogic(serviceNode1);

         log("Using remote service proxy of guest node 1:");
         RunHostClientLogic(serviceNode2);

         log("Using remote service proxy of guest node 2:");
         RunHostClientLogic(serviceNode3);
      }

      private void RunHostClientLogic(IServiceClient node) {
         Action<string> log = x => Debug.WriteLine("  N: " + x);

         log("Test Versioning Service");
         AssertEquals(node.GetService<IVersioningService>().GetVersion(), kVersioningServiceVersion);

         log("Test Login Service");
         AssertEquals(node.GetService<ILoginService>().GetStatus(), kLoginServiceStatus);

         log("Test Shop Service");
         AssertEquals(node.GetService<IShopService>().GetStatus(), kShopServiceStatus);

         log("Test Queue Service");
         AssertEquals(node.GetService<IQueueService>().GetWaitTimeMillis(), kQueueServiceWaitTimeMillis);
      }

      [Guid(kVersioningServiceGuid)]
      public interface IVersioningService {
         string GetVersion();
      }

      public class VersioningService : IVersioningService {
         public string GetVersion() {
            Debug.WriteLine("VersioningService: Invoked GetVersion()!");
            return kVersioningServiceVersion;
         }
      }

      [Guid(kLoginServiceGuid)]
      public interface ILoginService {
         string GetStatus();
      }

      public class LoginService : ILoginService {
         public string GetStatus() {
            Debug.WriteLine("LoginService: Invoked GetStatus()!");
            return kLoginServiceStatus;
         }
      }

      [Guid(kQueueServiceGuid)]
      public interface IQueueService {
         string GetWaitTimeMillis();
      }

      public class QueueService : IQueueService {
         public string GetWaitTimeMillis() {
            Debug.WriteLine("QueueService: Invoked GetWaitTimeMillis()!");
            return kQueueServiceWaitTimeMillis;
         }
      }

      [Guid(kShopServiceGuid)]
      public interface IShopService {
         string GetStatus();
      }

      public class ShopService : IShopService {
         public string GetStatus() {
            Debug.WriteLine("ShopService: Invoked GetStatus()!");
            return kShopServiceStatus;
         }
      }
   }
}
