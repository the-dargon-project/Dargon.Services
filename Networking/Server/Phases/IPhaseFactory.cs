using ItzWarty.Networking;

namespace Dargon.Services.Networking.Server.Phases {
   public interface IPhaseFactory {
      IPhase CreateIndeterminatePhase();
      IPhase CreateHostPhase(IListenerSocket listenerSocket);
      IPhase CreateGuestPhase(IConnectedSocket clientSocket);
   }
}
