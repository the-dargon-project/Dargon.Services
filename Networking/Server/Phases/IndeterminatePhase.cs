using System.Net.Sockets;
using ItzWarty;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Networking.Server.Phases {
   public class IndeterminatePhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kRetryInterval = 1000;

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;
      private readonly IServiceConfiguration configuration;
      private readonly IContext context;

      public IndeterminatePhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, IServiceConfiguration configuration, IContext context) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.configuration = configuration;
         this.context = context;
      }

      public void HandleUpdate() {
         IListenerSocket listenerSocket = null;
         IConnectedSocket clientSocket = null;
         var connectEndpoint = networkingProxy.CreateLoopbackEndPoint(configuration.Port);
         while (listenerSocket == null && clientSocket == null) {
            if (Util.IsThrown<SocketException>(() => { listenerSocket = networkingProxy.CreateListenerSocket(configuration.Port); })) {
               if (Util.IsThrown<SocketException>(() => { clientSocket = networkingProxy.CreateConnectedSocket(connectEndpoint); })) {
                  logger.Warn("Unable to either listen or connect to port " + configuration.Port);
                  threadingProxy.Sleep(kRetryInterval);
               }
            }
         }

         if (listenerSocket != null) {
            context.Transition(phaseFactory.CreateHostPhase(listenerSocket));
         } else {
            context.Transition(phaseFactory.CreateGuestPhase(clientSocket));
         }
      }

      public void Dispose() {
         // does nothing
      }
   }
}