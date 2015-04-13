using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.PortableObjects;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Common {
   public class InvocationManagerFactory : IInvocationManagerFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly ISocketFactory socketFactory;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly IInvocationStateFactory invocationStateFactory;
      private readonly IPofSerializer pofSerializer;

      public InvocationManagerFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, ISocketFactory socketFactory, PofStreamsFactory pofStreamsFactory, IInvocationStateFactory invocationStateFactory, IPofSerializer pofSerializer) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.socketFactory = socketFactory;
         this.pofStreamsFactory = pofStreamsFactory;
         this.invocationStateFactory = invocationStateFactory;
         this.pofSerializer = pofSerializer;
      }

      public IUserInvocationManager Create(ITcpEndPoint endpoint) {

         var invocationManager = new InvocationManagerImpl(collectionFactory, threadingProxy, invocationStateFactory);


         // HACK: Send handshake
         pofSerializer.Serialize(socket.GetWriter().__Writer, new X2SHandshake(Role.Client));

         var reader = new MessageReader(threadingProxy, pofSerializer, invocationManager, socket.GetReader());
         var writer = new MessageWriter(threadingProxy, pofSerializer, invocationManager, socket.GetWriter());
         reader.Initialize();
         writer.Initialize();
         invocationManager.SetReader(reader);
         invocationManager.SetWriter(writer);

         pofDispatcher.Start();
         return invocationManager;
      }
   }

   public interface IInvocationManagerFactory {
      IUserInvocationManager Create(ITcpEndPoint endpoint);
   }
}
