using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Sessions {
   public class HostGuestSession : HostSessionBase, IGuestSession {
      private int invocationCounter = 0;

      public HostGuestSession(IThread thread, ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IConnectedSocket socket) 
         : base(collectionFactory, pofSerializer, hostContext, socket, thread) {
      }

      public override Role Role { get { return Role.Guest; } }
   }
}