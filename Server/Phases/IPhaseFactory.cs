using ItzWarty.Networking;

namespace Dargon.Services.Server.Phases {
   public interface IPhaseFactory {
      IPhase CreateIndeterminatePhase(IConnectorContext connectorContext);
      IPhase CreateHostPhase(IConnectorContext connectorContext, IListenerSocket listenerSocket);
      IPhase CreateGuestPhase(IConnectorContext connectorContext, IConnectedSocket clientSocket);
   }
}
