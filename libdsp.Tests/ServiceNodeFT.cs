using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Server;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using Dargon.Services.Server.Sessions;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ServiceNodeFT : NMockitoInstance {
      private readonly ISynchronizationFactory synchronizationFactory;
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly IServiceNodeFactory serviceNodeFactory;
      private readonly INetworkingProxy networkingProxy;
      private const int kTestPort = 20001;
      private const int kHeartBeatIntervalMilliseconds = 30000;
      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";
      private readonly IServiceConfiguration serviceConfiguration = new ServiceConfiguration(kTestPort, kHeartBeatIntervalMilliseconds);

      public ServiceNodeFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new DspPofContext();
         pofSerializer = new PofSerializer(pofContext);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(collectionFactory, pofSerializer);
         IPhaseFactory phaseFactory = new PhaseFactory(collectionFactory, threadingProxy, networkingProxy, hostSessionFactory, pofSerializer);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, phaseFactory);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         serviceNodeFactory = new ServiceNodeFactory(connectorFactory, serviceContextFactory, collectionFactory);
      }

      [Fact]
      public void Run() {
         Action<string> log = (x) => Debug.WriteLine("S: " + x);

         log("Spawning Service Node.");
         var serviceNode = serviceNodeFactory.CreateOrJoin(serviceConfiguration);
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

         var handshake = new X2SHandshake(Role.Client);
         pofSerializer.Serialize(client.GetWriter().__Writer, handshake);
         log("Sent handshake to server.");

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
