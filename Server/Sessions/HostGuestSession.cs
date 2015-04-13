using System;
using Dargon.PortableObjects;
using Dargon.Services.Common;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Sessions {
   public class HostGuestSession : HostSessionBase, IGuestSession {
      private readonly IConcurrentSet<Guid> advertisedRemoteServiceIds;
      private readonly IUniqueIdentificationSet availableInvocationIds;
      private readonly IInvocationStateFactory invocationStateFactory;
      private readonly IConcurrentDictionary<uint, IInvocationState> pendingInvocationsById;

      public HostGuestSession(IThread thread, ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IConnectedSocket socket, IInvocationStateFactory invocationStateFactory) 
         : base(collectionFactory, pofSerializer, hostContext, socket, thread) {
         this.invocationStateFactory = invocationStateFactory;

         RegisterMessageHandler<G2HServiceBroadcast>(HandleServiceBroadcast);
         RegisterMessageHandler<G2HServiceUpdate>(HandleServiceUpdate);

         this.advertisedRemoteServiceIds = collectionFactory.CreateConcurrentSet<Guid>();
         this.availableInvocationIds = collectionFactory.CreateUniqueIdentificationSet(true);
         this.pendingInvocationsById = collectionFactory.CreateConcurrentDictionary<uint, IInvocationState>();
      }

      public override Role Role { get { return Role.Guest; } }

      public bool TryHandleInvocation(Guid serviceGuid, string methodName, object[] methodArguments, out object result) {
         if (advertisedRemoteServiceIds.Contains(serviceGuid)) {
            var invocationId = availableInvocationIds.TakeUniqueID();
            var invocationState = invocationStateFactory.Create(invocationId, serviceGuid, methodName, methodArguments);
            //pofSerializer.Serialize(writer, new X2XServiceInvocation())
         }
         throw new NotImplementedException();
      }

      public void HandleServiceBroadcast(G2HServiceBroadcast obj) {
         throw new NotImplementedException();
      }

      public void HandleServiceUpdate(G2HServiceUpdate x) {

      }
   }
}