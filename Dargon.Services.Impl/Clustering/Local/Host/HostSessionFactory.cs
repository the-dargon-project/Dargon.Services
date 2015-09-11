using System;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Clustering.Local.Host {
   public interface HostSessionFactory {
      HostSession Create(HostContext hostContext, IConnectedSocket socket);
   }

   public class HostSessionFactoryImpl : HostSessionFactory {
      private readonly IThreadingProxy threadingProxy;
      private readonly ICollectionFactory collectionFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public HostSessionFactoryImpl(IThreadingProxy threadingProxy, ICollectionFactory collectionFactory, IPofSerializer pofSerializer, PofStreamsFactory pofStreamsFactory, PortableObjectBoxConverter portableObjectBoxConverter) {
         this.threadingProxy = threadingProxy;
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }

      public HostSession Create(HostContext hostContext, IConnectedSocket socket) {
         var shutdownCancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         var pofStream = pofStreamsFactory.CreatePofStream(socket.Stream);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);
         var messageSender = new MessageSenderImpl(pofStream.Writer, portableObjectBoxConverter);
         var session = new HostSessionImpl(
            hostContext,
            shutdownCancellationTokenSource,
            messageSender,
            pofDispatcher,
            collectionFactory.CreateConcurrentSet<Guid>(),
            collectionFactory.CreateUniqueIdentificationSet(true),
            collectionFactory.CreateConcurrentDictionary<uint, AsyncValueBox>()
         );
         session.Initialize();
         return session;
      }
   }
}
