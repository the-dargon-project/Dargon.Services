using ItzWarty.Networking;

namespace Dargon.Services.Server.Phases {
   public interface IPhaseFactory {
      IPhase CreateIndeterminatePhase();
      IPhase CreateHostPhase(IListenerSocket listenerSocket);
      IPhase CreateGuestPhase(IConnectedSocket clientSocket);
   }
}
