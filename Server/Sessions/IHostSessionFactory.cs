using System;
using System.IO;
using Dargon.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Sessions {
   public interface IHostSessionFactory {
      IClientSession CreateClientSession(IThread thread, IHostContext hostContext, IConnectedSocket socket);
      IGuestSession CreateGuestSession(IThread thread, IHostContext hostContext, IConnectedSocket socket);
   }

   public class HostSessionFactory : IHostSessionFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IPofSerializer pofSerializer;

      public HostSessionFactory(ICollectionFactory collectionFactory, IPofSerializer pofSerializer) {
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
      }

      public IClientSession CreateClientSession(IThread thread, IHostContext hostContext, IConnectedSocket socket) {
         return new HostClientSession(thread, collectionFactory, pofSerializer, hostContext, socket);
      }

      public IGuestSession CreateGuestSession(IThread thread, IHostContext hostContext, IConnectedSocket socket) {
         throw new NotImplementedException();
         //return new HostGuestSession(thread, collectionFactory, pofSerializer, hostContext, socket);
      }
   }
}
