using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using Dargon.Services.PortableObjects;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using System.Threading;
using Xunit;

namespace Dargon.Services {
   public class ServiceClientFT : NMockitoInstance {
      private readonly ISynchronizationFactory synchronizationFactory;
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly ISocketFactory socketFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly IServiceClientFactory serviceClientFactory;
      private const int kTestPort = 20001;
      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";

      public ServiceClientFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(proxyGenerator);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         IInvocationStateFactory invocationStateFactory = new InvocationStateFactory(threadingProxy);
         IPofContext pofContext = new DspPofContext();
         pofSerializer = new PofSerializer(pofContext);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, socketFactory, invocationStateFactory, pofSerializer);
         serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, serviceContextFactory, connectorFactory);
      }

      [Fact]
      public void Run() {
         Action<string> log = (x) => Debug.WriteLine("C: " + x);
         var localEndpoint = tcpEndPointFactory.CreateLoopbackEndPoint(kTestPort);
         var synchronization = synchronizationFactory.CreateCountdownEvent(1);

         log("Spawning server thread...");
         var serverThread = new Thread(() => ServerThreadStart(localEndpoint, synchronization)).With(x => x.Start());

         synchronization.Wait();
         log("Server thread signaled ready to accept client.");

         var client = serviceClientFactory.Create(localEndpoint);
         var versioningService = client.GetService<IVersioningService>();

         log("Sending versioning service GetVersion invocation.");
         var version = versioningService.GetVersion();
         AssertEquals(kVersioningServiceVersion, version);
         log("Validated GetVersion invocation result.");

         client.Dispose();
         log("Finished disposing resources.");

         serverThread.Join();
         log("Joined with server thread.");
      }

      private void ServerThreadStart(ITcpEndPoint localEndpoint, ICountdownEvent synchronization) {
         Action<string> log = (x) => Debug.WriteLine("S: " + x);
         log("Enter Server Thread.");
         var listener = socketFactory.CreateListenerSocket(localEndpoint);
         synchronization.Signal();
         log("Signal ready for client.");
         
         var client = listener.Accept();
         log("Accepted client.");

         var handshake = pofSerializer.Deserialize<X2SHandshake>(client.GetReader().__Reader);
         AssertEquals(Role.Client, handshake.Role);
         log("Received and validated client handshake.");
         
         var request = pofSerializer.Deserialize<C2HServiceInvocation>(client.GetReader().__Reader);
         AssertEquals(Guid.Parse(kVersioningServiceGuid), request.ServiceGuid);
         AssertEquals("GetVersion", request.MethodName);
         AssertEquals(0, request.MethodArguments.Length);
         log("Received and validated client service invocation.");
         
         var response = new H2CInvocationResult(request.InvocationId, kVersioningServiceVersion);
         pofSerializer.Serialize(client.GetWriter().__Writer, response);
         log("Sent response to client service invocation.");

         listener.Dispose();
         client.Dispose();
         log("Finished disposing resources.");
      }

      [Guid(kVersioningServiceGuid)]
      public interface IVersioningService {
         string GetVersion();
      }
   }
}
