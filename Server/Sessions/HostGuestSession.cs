using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Server.Sessions {
   public class HostGuestSession : HostSessionBase, IGuestSession {
      private int invocationCounter = 0;

      public HostGuestSession(
         ICollectionFactory collectionFactory, 
         IPofSerializer pofSerializer, 
         IHostContext hostContext, 
         IBinaryReader reader, 
         IBinaryWriter writer
      ) : base(collectionFactory, pofSerializer, hostContext, reader, writer) {
      }

      public override Role Role { get { return Role.Guest; } }
   }
}