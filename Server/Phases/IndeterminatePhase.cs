using System.Net.Sockets;
using ItzWarty;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Server.Phases {
   public class IndeterminatePhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kRetryInterval = 1000;

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;
      private readonly IConnectorContext connectorContext;

      public IndeterminatePhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, IConnectorContext connectorContext) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.connectorContext = connectorContext;
      }

      public void HandleEnter() {
         IListenerSocket listener = null;
         IConnectedSocket client = null;
         var configuration = connectorContext.ServiceConfiguration;
         var connectEndpoint = networkingProxy.CreateLoopbackEndPoint(configuration.Port);
         while (listener == null && client == null) {
            if (Util.IsThrown<SocketException>(() => { listener = networkingProxy.CreateListenerSocket(configuration.Port); })) {
               if (Util.IsThrown<SocketException>(() => { client = networkingProxy.CreateConnectedSocket(connectEndpoint); })) {
                  logger.Warn("Unable to either listen or connect to port " + configuration.Port);
                  threadingProxy.Sleep(kRetryInterval);
               }
            }
         }

         if (listener != null) {
            connectorContext.Transition(phaseFactory.CreateHostPhase(connectorContext, listener));
         } else {
            connectorContext.Transition(phaseFactory.CreateGuestPhase(connectorContext, client));
         }
      }

      public void HandleServiceRegistered(IServiceContext serviceContext) {
         // does nothing
      }

      public void HandleServiceUnregistered(IServiceContext serviceContext) {
         // does nothing
      }

      public void Dispose() {
         // does nothing
      }
   }
}