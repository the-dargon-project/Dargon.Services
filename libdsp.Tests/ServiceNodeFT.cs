using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
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
using Dargon.PortableObjects.Streams;
using Dargon.Services.Clustering.Host;
using Xunit;

namespace Dargon.Services {
   public class ServiceNodeFT : NMockitoInstance {
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly IServiceClientFactory serviceClientFactory;
      private readonly INetworkingProxy networkingProxy;
      private const int kTestPort = 20001;
      private const int kHeartBeatIntervalMilliseconds = 30000;
      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";
      private readonly IClusteringConfiguration clusteringConfiguration = new ClusteringConfiguration(kTestPort, kHeartBeatIntervalMilliseconds);

      public ServiceNodeFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new DspPofContext();
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory);
         pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory);
         serviceClientFactory = new ServiceClientFactory(collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, invokableServiceContextFactory);
      }

      [Fact]
      public void Run() {
         Action<string> log = (x) => Debug.WriteLine("S: " + x);

         log("Spawning Service Node.");
         var serviceNode = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         var versioningService = new VersioningService();

         log("Registering Versioning Service to Service Node.");
         serviceNode.RegisterService(versioningService, typeof(IVersioningService));

         log("Spawning client thread...");

         // We actually run this in the same thread so thrown exceptions work.
         log("Begin client logic...");
         RunClientLogic();
         log("Client logic completed...");

         log("Killing service node...");
         serviceNode.Dispose();
         log("Finished disposing resources...");
      }

      private void RunClientLogic() {
         Action<string> log = (x) => Debug.WriteLine("C: " + x);
         log("Enter Client Thread.");

         var endpoint = tcpEndPointFactory.CreateLoopbackEndPoint(kTestPort);
         var client = networkingProxy.CreateConnectedSocket(endpoint);
         log("Connected to server.");

//         var handshake = new X2SHandshake(Role.Client);
//         pofSerializer.Serialize(client.GetWriter().__Writer, handshake);
//         log("Sent handshake to server.");

         const int invocationId = 0;
         const string methodName = "GetVersion";
         var methodArguments = new object[0];
         var invocation = new X2XServiceInvocation(invocationId, Guid.Parse(kVersioningServiceGuid), methodName, methodArguments);
         pofSerializer.Serialize(client.GetWriter().__Writer, invocation);
         log("Sent invocation to server.");

         log("Awaiting invocation result from server.");
         var invocationResult = pofSerializer.Deserialize<X2XInvocationResult>(client.GetReader().__Reader);
         AssertEquals(kVersioningServiceVersion, invocationResult.Payload);
         log("Received and validated invocation result from server!");
         Thread.Sleep(1000);
         log("Signaled Thread Completion.");
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
   }
}
