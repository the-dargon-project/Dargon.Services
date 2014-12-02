using System.Linq;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using Dargon.Services.Server.Phases;
using Dargon.Services.Server.Sessions;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Dargon.Services {
   public class ServiceNodeAndClientFT : NMockitoInstance {
      private readonly ITcpEndPointFactory tcpEndPointFactory;
      private readonly IServiceNodeFactory serviceNodeFactory;
      private readonly IServiceClientFactory serviceClientFactory;
      private const int kTestPort = 20001;
      private const int kHeartBeatIntervalMilliseconds = 30000;
      private const string kVersioningServiceGuid = "1D98294F-FA5A-472F-91F7-2A96CF973531";
      private const string kVersioningServiceVersion = "123.343.5-asdf";
      private readonly IServiceConfiguration serviceConfiguration = new ServiceConfiguration(kTestPort, kHeartBeatIntervalMilliseconds);

      public ServiceNodeAndClientFT() {
         var proxyGenerator = new ProxyGenerator();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(proxyGenerator);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IInvocationStateFactory invocationStateFactory = new InvocationStateFactory(threadingProxy);
         IPofContext pofContext = new DspPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(collectionFactory, pofSerializer);
         IPhaseFactory phaseFactory = new PhaseFactory(collectionFactory, threadingProxy, networkingProxy, hostSessionFactory, pofSerializer);
         Server.IConnectorFactory serverConnectorFactory = new Server.ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, phaseFactory);
         Client.IConnectorFactory clientConnectorFactory = new Client.ConnectorFactory(collectionFactory, threadingProxy, socketFactory, invocationStateFactory, pofSerializer);
         Server.IServiceContextFactory serverServiceContextFactory = new Server.ServiceContextFactory(collectionFactory);
         Client.IServiceContextFactory clientServiceContextFactory = new Client.ServiceContextFactory(collectionFactory);
         serviceNodeFactory = new ServiceNodeFactory(serverConnectorFactory, serverServiceContextFactory, collectionFactory);
         serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, clientServiceContextFactory, clientConnectorFactory);
      }

      [Fact]
      public void Run() {
         var serviceNode = serviceNodeFactory.CreateOrJoin(serviceConfiguration);
         serviceNode.RegisterService(new VersioningService(), typeof(IVersioningService));
         
         var localEndpoint = tcpEndPointFactory.CreateLoopbackEndPoint(kTestPort);
         var serviceClient = serviceClientFactory.Create(localEndpoint);
         var versioningService = serviceClient.GetService<IVersioningService>();

         AssertEquals(kVersioningServiceVersion, versioningService.GetVersion());
         AssertTrue(versioningService.GetTags().SequenceEqual(new[] { "Prerelease", "Beta" }));
         AssertTrue(versioningService.GetTagsArray().SequenceEqual(new[] { "Prerelease", "Beta" }));

         serviceNode.Dispose();
         serviceClient.Dispose();
         Debug.WriteLine("Exited cleanly");
      }

      [Guid(kVersioningServiceGuid)]
      public interface IVersioningService {
         string GetVersion();
         IEnumerable<string> GetTags();
         string[] GetTagsArray();
      }

      public class VersioningService : IVersioningService {
         public string GetVersion() {
            Debug.WriteLine("VersioningService: Invoked GetVersion()!");
            return kVersioningServiceVersion;
         }

         public IEnumerable<string> GetTags() {
            yield return "Prerelease";
            yield return "Beta";
         }

         public string[] GetTagsArray() {
            return new string[] { "Prerelease", "Beta" };
         }
      }
   }
}
