using Dargon.Services.Server;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;
using System.Net.Sockets;

namespace Dargon.Services.Phases.Indeterminate {
   public class IndeterminateClusteringPhase : ClusteringPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kRetryInterval = 1000;

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly ClusteringPhaseFactory clusteringPhaseFactory;
      private readonly IClusteringConfiguration clusteringConfiguration;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;

      public IndeterminateClusteringPhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, ClusteringPhaseFactory clusteringPhaseFactory, IClusteringConfiguration clusteringConfiguration, LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.clusteringPhaseFactory = clusteringPhaseFactory;
         this.clusteringConfiguration = clusteringConfiguration;
         this.localServiceContainer = localServiceContainer;
         this.clusteringPhaseManager = clusteringPhaseManager;
      }

      public void HandleEnter() {
         IListenerSocket listener = null;
         IConnectedSocket client = null;
         var connectEndpoint = networkingProxy.CreateLoopbackEndPoint(clusteringConfiguration.Port);
         var hostAllowed = !clusteringConfiguration.ClusteringRoleFlags.HasFlag(ClusteringRoleFlags.GuestOnly);
         var guestAllowed = !clusteringConfiguration.ClusteringRoleFlags.HasFlag(ClusteringRoleFlags.HostOnly);
         while (listener == null && client == null) {
            if (hostAllowed && TryCreateHostListener(clusteringConfiguration, out listener)) {
               break;
            }
            if (guestAllowed && TryCreateGuestSocket(connectEndpoint, out client)) {
               break;
            }
            logger.Warn("Unable to either listen/connect to port " + clusteringConfiguration.Port);
            threadingProxy.Sleep(kRetryInterval);
         }

         if (listener != null) {
            clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateHostPhase(localServiceContainer, listener));
         } else {
            clusteringPhaseManager.Transition(clusteringPhaseFactory.CreateGuestPhase(localServiceContainer, client));
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