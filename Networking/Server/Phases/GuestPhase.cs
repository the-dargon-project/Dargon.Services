using System;
using System.IO;
using ItzWarty;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Networking.Server.Phases {
   public class GuestPhase : IPhase {
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPhaseFactory phaseFactory;
      private readonly IContext context;
      private readonly IConnectedSocket socket;

      public GuestPhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPhaseFactory phaseFactory, IContext context, IConnectedSocket socket) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.phaseFactory = phaseFactory;
         this.context = context;
         this.socket = socket;
      }

      public void Initialize() {

      }

      public void HandleUpdate() {
      }

      public void Dispose() {
         throw new NotImplementedException();
      }
   }
}