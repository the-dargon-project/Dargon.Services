using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Networking.Server.Sessions {
   public class HostGuestSession : HostSessionBase, IGuestSession {
      private int invocationCounter = 0;

      public HostGuestSession(ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IBinaryReader reader, IBinaryWriter writer) 
         : base(collectionFactory, pofSerializer, hostContext, reader, writer, Role.Guest) {
      }

   }
}