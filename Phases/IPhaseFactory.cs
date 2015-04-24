using Dargon.Services.Server;
using ItzWarty.Networking;

namespace Dargon.Services.Phases {
   public interface IPhaseFactory {
      IPhase CreateIndeterminatePhase(LocalServiceContainer localServiceContainer);
      IPhase CreateHostPhase(LocalServiceContainer localServiceContainer, IListenerSocket listenerSocket);
      IPhase CreateGuestPhase(LocalServiceContainer localServiceContainer, IConnectedSocket clientSocket);
   }
}
