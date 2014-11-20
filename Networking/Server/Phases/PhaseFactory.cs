using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Networking.Server.Phases {
   public class PhaseFactory : IPhaseFactory {
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IServiceConfiguration serviceConfiguration;
      private readonly IContext context;

      public PhaseFactory(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer,  IServiceConfiguration serviceConfiguration, IContext context) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.serviceConfiguration = serviceConfiguration;
         this.context = context;
      }

      public IPhase CreateIndeterminatePhase() {
         return new IndeterminatePhase(threadingProxy, networkingProxy, this, serviceConfiguration, context);
      }

      public IPhase CreateHostPhase(IListenerSocket listenerSocket) {
         return new HostPhase(threadingProxy, networkingProxy, pofSerializer, context, listenerSocket).With(x => x.Initialize());
      }

      public IPhase CreateGuestPhase(IConnectedSocket clientSocket) {
         return new GuestPhase(threadingProxy, networkingProxy, this, context, clientSocket).With(x => x.Initialize());
      }
   }
}
