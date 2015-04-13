using System.Runtime.Remoting.Messaging;
using Dargon.PortableObjects.Streams;
using ItzWarty.Networking;

namespace Dargon.Services.Common {
   public interface MyNameIsItzWartyFactory {
      SomeConnectionContext Create(ITcpEndPoint endpoint);
   }

   public class MyNameIsItzWartyFactoryImpl : MyNameIsItzWartyFactory {
      private readonly ISocketFactory socketFactory;
      private readonly PofStreamsFactory pofStreamsFactory;

      public MyNameIsItzWartyFactoryImpl(ISocketFactory socketFactory, PofStreamsFactory pofStreamsFactory) {
         this.socketFactory = socketFactory;
         this.pofStreamsFactory = pofStreamsFactory;
      }

      public SomeConnectionContext Create(ITcpEndPoint endpoint) {
         var socket = socketFactory.CreateConnectedSocket(endpoint);
         var pofStream = pofStreamsFactory.CreatePofStream(socket.Stream);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);
         return new SomeConnectionContextImpl(socket, pofStream, pofDispatcher);
      }
   }
}