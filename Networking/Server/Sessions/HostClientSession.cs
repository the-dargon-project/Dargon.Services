using System;
using System.Collections;
using System.Collections.Generic;
using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Networking.Server.Sessions {
   public class HostClientSession : HostSessionBase, IClientSession {
      public HostClientSession(ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IBinaryReader reader, IBinaryWriter writer)
         : base(collectionFactory, pofSerializer, hostContext, reader, writer, Role.Client) { 
         RegisterMessageHandler<C2HServiceInvocation>(HandleC2HServiceInvocation);
      }

      private void HandleC2HServiceInvocation(C2HServiceInvocation message) {
         if (message == null) {
            var error = new ArgumentException("Expected " + typeof(C2HServiceInvocation).FullName + " got " + (obj == null ? "null" : obj.GetType().FullName));
            pofSerializer.Serialize(writer.__Writer, new H2CInvocationResult(uint.MaxValue, new PortableException(error)));
         } else {
            try {
               var result = hostContext.Invoke(message.ServiceGuid, message.MethodName, message.MethodArguments);
               pofSerializer.Serialize(writer.__Writer, new H2CInvocationResult(message.InvocationId, result));
            } catch (Exception e) {
               pofSerializer.Serialize(writer.__Writer, new H2CInvocationResult(message.InvocationId, new PortableException(e)));
            }
         }
      }
   }
}