using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ClusteredServiceNodeFT : NMockitoInstance {
      private const int kTestPort = 20001;

      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersionString = "123.343.5-asdf";
      private const string kLoginServiceGuid = "E5C6A3A0-958A-48F6-9875-DF0C4FA561C1";
      private const string kLoginServiceStatus = "Okay";
      private const string kQueueServiceGuid = "69EE0CAF-B105-492C-9DF2-851F6207264C";
      private const string kQueueServiceWaitTimeMillis = "12345";
      private const string kShopServiceGuid = "EFBE6150-CC18-441C-9757-23C41823F8C3";
      private const string kShopServiceStatus = "Okay";

      private ServiceClientFactoryImpl CreateServiceClientFactory(params PofContext[] pofContexts) {
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
         PofContext pofContext = new DspPofContext();
         pofContexts.ForEach(pofContext.MergeContext);
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         PortableObjectBoxConverter portableObjectBoxConverter = new PortableObjectBoxConverter(streamFactory, pofSerializer);
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory, portableObjectBoxConverter);
         return new ServiceClientFactoryImpl(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
      }

      [Fact]
      public void Run() {
         Action<string> log = x => Debug.WriteLine("T: " + x);
         log("Spawning Service Node 1.");
         var serviceNode1 = CreateServiceClientFactory(new VersioningServicePofContext(), new LoginServicePofContext()).Local(kTestPort);
         serviceNode1.RegisterService(new VersioningService(), typeof(IVersioningService));

         log("Spawning Service Node 2.");
         var serviceNode2 = CreateServiceClientFactory(new VersioningServicePofContext(), new LoginServicePofContext(), new QueueServicePofContext()).Local(kTestPort);
         serviceNode2.RegisterService(new LoginService(), typeof(ILoginService));
         serviceNode2.RegisterService(new ShopService(), typeof(IShopService));

         log("Spawning Service Node 3.");
         var serviceNode3 = CreateServiceClientFactory(new LoginServicePofContext(), new QueueServicePofContext()).Local(kTestPort);
         serviceNode3.RegisterService(new QueueService(), typeof(IQueueService));

         // Give 500ms for nodes to discover services.
         Thread.Sleep(500);

         log("Using remote service proxy of host node:");
         RunHostClientLogic(serviceNode1, true, true, true, false);

         log("Using remote service proxy of guest node 1:");
         RunHostClientLogic(serviceNode2, true, true, true, true);

         log("Using remote service proxy of guest node 2:");
         RunHostClientLogic(serviceNode3, false, false, true, true);
      }

      private void RunHostClientLogic(ServiceClient node, bool testVersioning, bool testLogin, bool testShop, bool testQueue) {
         Action<string> log = x => Debug.WriteLine("  N: " + x);

         if (testVersioning) {
            log("Test Versioning Service");
            AssertEquals(node.GetService<IVersioningService>().GetVersion().FriendlyName, kVersioningServiceVersionString);
         }

         if (testLogin) {
            log("Test Login Service");
            AssertEquals(node.GetService<ILoginService>().GetStatus().StatusCode, kLoginServiceStatus);
         }

         if (testShop) {
            log("Test Shop Service");
            AssertEquals(node.GetService<IShopService>().GetStatus(), kShopServiceStatus);
         }

         if (testQueue) {
            log("Test Queue Service");
            AssertEquals(node.GetService<IQueueService>().GetWaitTimeMillis().Milliseconds, kQueueServiceWaitTimeMillis);
         }
      }

      [Guid(kVersioningServiceGuid)]
      public interface IVersioningService {
         VersioningServiceVersion GetVersion();
      }

      public class VersioningService : IVersioningService {
         public VersioningServiceVersion GetVersion() {
            Debug.WriteLine("VersioningService: Invoked GetVersion()!");
            return new VersioningServiceVersion(kVersioningServiceVersionString);;
         }
      }

      public class VersioningServiceVersion : IPortableObject {
         private string friendlyName;

         public VersioningServiceVersion() { }

         public VersioningServiceVersion(string friendlyName) {
            this.friendlyName = friendlyName;
         }

         public string FriendlyName { get { return friendlyName; } }

         public void Serialize(IPofWriter writer) => writer.WriteString(0, friendlyName);
         public void Deserialize(IPofReader reader) => friendlyName = reader.ReadString(0);
      }

      public class VersioningServicePofContext : PofContext {
         public VersioningServicePofContext() {
            RegisterPortableObjectType(100, typeof(VersioningServiceVersion));
         }
      }

      [Guid(kLoginServiceGuid)]
      public interface ILoginService {
         LoginServiceStatus GetStatus();
      }

      public class LoginService : ILoginService {
         public LoginServiceStatus GetStatus() {
            Debug.WriteLine("LoginService: Invoked GetStatus()!");
            return new LoginServiceStatus(kLoginServiceStatus);
         }
      }

      public class LoginServiceStatus : IPortableObject {
         private string statusCode;

         public LoginServiceStatus() { }

         public LoginServiceStatus(string statusCode) {
            this.statusCode = statusCode;
         }

         public string StatusCode => statusCode;

         public void Serialize(IPofWriter writer) => writer.WriteString(0, statusCode);
         public void Deserialize(IPofReader reader) => statusCode = reader.ReadString(0);
      }

      public class LoginServicePofContext : PofContext {
         public LoginServicePofContext() {
            RegisterPortableObjectType(200, typeof(LoginServiceStatus));
         }
      }

      [Guid(kQueueServiceGuid)]
      public interface IQueueService {
         QueueServiceWaitTime GetWaitTimeMillis();
      }

      public class QueueService : IQueueService {
         public QueueServiceWaitTime GetWaitTimeMillis() {
            Debug.WriteLine("QueueService: Invoked GetWaitTimeMillis()!");
            return new QueueServiceWaitTime(kQueueServiceWaitTimeMillis);
         }
      }

      public class QueueServiceWaitTime : IPortableObject {
         private string milliseconds;

         public QueueServiceWaitTime() { }

         public QueueServiceWaitTime(string milliseconds) {
            this.milliseconds = milliseconds;
         }

         public string Milliseconds => milliseconds;

         public void Serialize(IPofWriter writer) => writer.WriteString(0, milliseconds);
         public void Deserialize(IPofReader reader) => milliseconds = reader.ReadString(0);
      }

      public class QueueServicePofContext : PofContext {
         public QueueServicePofContext() {
            RegisterPortableObjectType(300, typeof(QueueServiceWaitTime));
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
