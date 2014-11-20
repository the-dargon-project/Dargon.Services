using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public class ConnectorContext : IContext {
      private IPhase phase;
      private IConcurrentDictionary<string, IServiceContext> serviceContextsByName;

      public ConnectorContext(IPhase initialPhase) {
         this.phase = initialPhase;
      }

      public IConcurrentDictionary<string, IServiceContext> ServiceContextsByName { get { return serviceContextsByName; } }
      public IPhase CurrentPhase { get { return phase; } }

      public void Initialize(IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.serviceContextsByName = serviceContextsByName;
      }

      public void Transition(IPhase phase) {
         this.phase = phase;
      }

      public void HandleUpdate() {
         this.phase.HandleUpdate();
      }
   }
}