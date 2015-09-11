using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dargon.Services.Server;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Clustering.Local.Indeterminate {
   public class IndeterminateClusteringPhase : ClusteringPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kRetryInterval = 1000;

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly ClusteringPhaseFactory clusteringPhaseFactory;
      private readonly ClusteringConfiguration clusteringConfiguration;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly ClusteringPhaseManager clusteringPhaseManager;
      private readonly SemaphoreSlim phaseTransitionedLatch = new SemaphoreSlim(0, int.MaxValue);

      public IndeterminateClusteringPhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, ClusteringPhaseFactory clusteringPhaseFactory, ClusteringConfiguration clusteringConfiguration, LocalServiceContainer localServiceContainer, ClusteringPhaseManager clusteringPhaseManager) {
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
         var connectEndpoint = networkingProxy.CreateEndPoint(clusteringConfiguration.Address, clusteringConfiguration.Port);
         var hostAllowed = !clusteringConfiguration.ClusteringRole.HasFlag(ClusteringRole.GuestOnly) && IPAddress.IsLoopback(clusteringConfiguration.Address);
         var guestAllowed = !clusteringConfiguration.ClusteringRole.HasFlag(ClusteringRole.HostOnly);
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
         phaseTransitionedLatch.Release(int.MaxValue);
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

      private bool TryCreateHostListener(ClusteringConfiguration configuration, out IListenerSocket listener) {
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

      public async Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments) {
         await phaseTransitionedLatch.WaitAsync();
         return await clusteringPhaseManager.InvokeServiceCall(serviceGuid, methodName, genericArguments, methodArguments);
      }

      public void Dispose() {
         // does nothing
      }
   }
}