using Dargon.Services.Server;
using ItzWarty.Networking;

namespace Dargon.Services.Phases {
   public interface IPhaseFactory {
      IPhase CreateIndeterminatePhase(IServiceNodeContext serviceNodeContext);
      IPhase CreateHostPhase(IServiceNodeContext serviceNodeContext, IListenerSocket listenerSocket);
      IPhase CreateGuestPhase(IServiceNodeContext serviceNodeContext, IConnectedSocket clientSocket);
   }
}
