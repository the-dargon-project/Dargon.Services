using System;
using System.Diagnostics;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Sessions {
   public class HostClientSession : HostSessionBase, IClientSession {
      public HostClientSession(IThread thread, ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IConnectedSocket socket)
         : base(collectionFactory, pofSerializer, hostContext, socket, thread) {
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
