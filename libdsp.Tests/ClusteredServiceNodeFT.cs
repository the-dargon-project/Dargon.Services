using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Phases.Host;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using ItzWarty;
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
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly IServiceClientFactory serviceClientFactory;
      private readonly INetworkingProxy networkingProxy;

      private const int kTestPort = 20001;
      private const int kHeartBeatIntervalMilliseconds = 30000;
      private readonly IClusteringConfiguration clusteringConfiguration = new ClusteringConfiguration(kTestPort, kHeartBeatIntervalMilliseconds);

      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";
      private const string kLoginServiceGuid = "E5C6A3A0-958A-48F6-9875-DF0C4FA561C1";
      private const string kLoginServiceStatus = "Okay";
      private const string kQueueServiceGuid = "69EE0CAF-B105-492C-9DF2-851F6207264C";
      private const string kQueueServiceWaitTimeMillis = "12345";

      public ClusteredServiceNodeFT() {
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
         Action<string> log = (x) => Debug.WriteLine("T: " + x);
         log("Spawning Service Node 1.");
         var serviceNode1 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode1.RegisterService(new VersioningService(), typeof(IVersioningService));

         log("Spawning Service Node 2.");
         var serviceNode2 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode2.RegisterService(new LoginService(), typeof(ILoginService));

         log("Spawning Service Node 3.");
         var serviceNode3 = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         serviceNode3.RegisterService(new QueueService(), typeof(IQueueService));

         // Give 500ms for nodes to discover services.
         Thread.Sleep(500);

         log("Running client logic.");
         RunClientLogic();
      }

      private void RunClientLogic() {
         Action<string> log = (x) => Debug.WriteLine("C: " + x);
         log("Enter Client Thread.");

         var endpoint = tcpEndPointFactory.CreateLoopbackEndPoint(kTestPort);
         var client = networkingProxy.CreateConnectedSocket(endpoint);
         log("Connected to host node.");

         var stopwatch = new Stopwatch();
         stopwatch.Start();

         TestInvokeHostNode1(client, log);
         TestInvokeGuestNode1(client, log);
         TestInvokeGuestNode2(client, log);

         log("Test Invocations completed after {0} ms.".F(stopwatch.ElapsedMilliseconds));

         log("Signaled Thread Completion.");
      }

      private void TestInvokeHostNode1(IConnectedSocket client, Action<string> log) {
         const int invocationId = 0;
         const string methodName = "GetVersion";
         var methodArguments = new object[0];
         var invocation = new X2XServiceInvocation(invocationId, Guid.Parse(kVersioningServiceGuid), methodName, methodArguments);
         pofSerializer.Serialize(client.GetWriter().__Writer, invocation);
         log("Sent versioning service invocation to server.");

         log("Awaiting versioning service invocation result from server.");
         var invocationResult = pofSerializer.Deserialize<X2XInvocationResult>(client.GetReader().__Reader);
         AssertEquals(kVersioningServiceVersion, invocationResult.Payload);
         log("Received and validated versioning service invocation result from server!");
      }

      private void TestInvokeGuestNode1(IConnectedSocket client, Action<string> log) {
         const int invocationId = 1;
         const string methodName = "GetStatus";
         var methodArguments = new object[0];
         var invocation = new X2XServiceInvocation(invocationId, Guid.Parse(kLoginServiceGuid), methodName, methodArguments);
         pofSerializer.Serialize(client.GetWriter().__Writer, invocation);
         log("Sent versioning service invocation to server.");

         log("Awaiting versioning service invocation result from server.");
         var invocationResult = pofSerializer.Deserialize<X2XInvocationResult>(client.GetReader().__Reader);
         AssertEquals(kLoginServiceStatus, invocationResult.Payload);
         log("Received and validated versioning service invocation result from server!");
      }

      private void TestInvokeGuestNode2(IConnectedSocket client, Action<string> log) {
         const int invocationId = 2;
         const string methodName = "GetWaitTimeMillis";
         var methodArguments = new object[0];
         var invocation = new X2XServiceInvocation(invocationId, Guid.Parse(kQueueServiceGuid), methodName, methodArguments);
         pofSerializer.Serialize(client.GetWriter().__Writer, invocation);
         log("Sent queue service invocation to server.");

         log("Awaiting queue service invocation result from server.");
         var invocationResult = pofSerializer.Deserialize<X2XInvocationResult>(client.GetReader().__Reader);
         AssertEquals(kQueueServiceWaitTimeMillis, invocationResult.Payload);
         log("Received and validated queue service invocation result from server!");
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
   }
}
