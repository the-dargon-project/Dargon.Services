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
         var hostAllowed = !configuration.NodeOwnershipFlags.HasFlag(NodeOwnershipFlags.GuestOnly);
         var guestAllowed = !configuration.NodeOwnershipFlags.HasFlag(NodeOwnershipFlags.HostOnly);
         while (listener == null && client == null) {
            if (hostAllowed && TryCreateHostListener(configuration, out listener)) {
               break;
            }
            if (guestAllowed && TryCreateGuestSocket(connectEndpoint, out client)) {
               break;
            }
            logger.Warn("Unable to either listen/connect to port " + configuration.Port);
            threadingProxy.Sleep(kRetryInterval);
         }

         if (listener != null) {
            connectorContext.Transition(phaseFactory.CreateHostPhase(connectorContext, listener));
         } else {
            connectorContext.Transition(phaseFactory.CreateGuestPhase(connectorContext, client));
         }
      }

      private bool TryCreateGuestSocket(ITcpEndPoint connectEndpoint, out IConnectedSocket client) {
         try {
            client = networkingProxy.CreateConnectedSocket(connectEndpoint);
            return true;
         } catch (SocketException) {
            client = null;
            return false;
         }
      }

      private bool TryCreateHostListener(IServiceConfiguration configuration, out IListenerSocket listener) {
         try {
            listener = networkingProxy.CreateListenerSocket(configuration.Port);
            return true;
         } catch (SocketException) {
            listener = null;
            return false;
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