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
      private readonly IServiceNodeContext serviceNodeContext;

      public IndeterminatePhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, IServiceNodeContext serviceNodeContext) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.serviceNodeContext = serviceNodeContext;
      }

      public void HandleEnter() {
         IListenerSocket listener = null;
         IConnectedSocket client = null;
         var configuration = serviceNodeContext.NodeConfiguration;
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
            serviceNodeContext.Transition(phaseFactory.CreateHostPhase(serviceNodeContext, listener));
         } else {
            serviceNodeContext.Transition(phaseFactory.CreateGuestPhase(serviceNodeContext, client));
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

      private bool TryCreateHostListener(INodeConfiguration configuration, out IListenerSocket listener) {
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