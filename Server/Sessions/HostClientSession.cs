using System;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Server.Sessions {
   public class HostClientSession : HostSessionBase, IClientSession {
      public HostClientSession(
         ICollectionFactory collectionFactory,
         IPofSerializer pofSerializer,
         IHostContext hostContext,
         IBinaryReader reader,
         IBinaryWriter writer
      ) : base(collectionFactory, pofSerializer, hostContext, reader, writer) {
         RegisterMessageHandler<C2HServiceInvocation>(HandleC2HServiceInvocation);
      }

      public override Role Role { get { return Role.Client; } }

      private void HandleC2HServiceInvocation(C2HServiceInvocation x) {
         object payload;
         try {
            payload = hostContext.Invoke(x.ServiceGuid, x.MethodName, x.MethodArguments);
         } catch (Exception e) {
            payload = new PortableException(e);
         }
         var result = new H2CInvocationResult(x.InvocationId, payload);
         pofSerializer.Serialize(writer.__Writer, result);
      }
   }
}
