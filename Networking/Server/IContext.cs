using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server {
   public interface IContext : IPhase {
      IConcurrentDictionary<string, IServiceContext> ServiceContextsByName { get; }
      IPhase CurrentPhase { get; }

      void Initialize(IConcurrentDictionary<string, IServiceContext> serviceContextsByName);
      void Transition(IPhase phase);
   }
}
