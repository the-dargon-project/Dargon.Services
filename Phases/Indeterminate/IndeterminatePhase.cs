using System.Net.Sockets;
using Dargon.Services.Server;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Phases.Indeterminate {
   public class IndeterminatePhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kRetryInterval = 1000;

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;
      private readonly LocalServiceContainer localServiceContainer;

      public IndeterminatePhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, LocalServiceContainer localServiceContainer) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.localServiceContainer = localServiceContainer;
      }

      public void HandleEnter() {
         IListenerSocket listener = null;
         IConnectedSocket client = null;
         var configuration = localServiceContainer.ClusteringConfiguration;
         var connectEndpoint = networkingProxy.CreateLoopbackEndPoint(configuration.Port);
         var hostAllowed = !configuration.ClusteringRoleFlags.HasFlag(ClusteringRoleFlags.GuestOnly);
         var guestAllowed = !configuration.ClusteringRoleFlags.HasFlag(ClusteringRoleFlags.HostOnly);
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
            localServiceContainer.Transition(phaseFactory.CreateHostPhase(localServiceContainer, listener));
         } else {
            localServiceContainer.Transition(phaseFactory.CreateGuestPhase(localServiceContainer, client));
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

      private bool TryCreateHostListener(IClusteringConfiguration configuration, out IListenerSocket listener) {
         try {
            listener = networkingProxy.CreateListenerSocket(configuration.Port);
            return true;
         } catch (SocketException) {
            listener = null;
            return false;
         }
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public void Dispose() {
         // does nothing
      }
   }
}