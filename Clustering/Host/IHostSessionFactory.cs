using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Clustering.Host {
   public interface IHostSessionFactory {
      IHostSession Create(IThread thread, IHostContext hostContext, IConnectedSocket socket);
   }

   public class HostSessionFactory : IHostSessionFactory {
      private readonly IThreadingProxy threadingProxy;
      private readonly ICollectionFactory collectionFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;

      public HostSessionFactory(IThreadingProxy threadingProxy, ICollectionFactory collectionFactory, IPofSerializer pofSerializer, PofStreamsFactory pofStreamsFactory) {
         this.threadingProxy = threadingProxy;
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
      }

      public IHostSession Create(IThread thread, IHostContext hostContext, IConnectedSocket socket) {
         var session = new HostSession(threadingProxy, collectionFactory, pofStreamsFactory, hostContext, socket, thread);
         session.Initialize();
         return session;
      }
   }
}
