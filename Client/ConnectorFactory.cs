using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public class ConnectorFactory : IConnectorFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly ISocketFactory socketFactory;
      private readonly IInvocationStateFactory invocationStateFactory;
      private readonly IPofSerializer pofSerializer;

      public ConnectorFactory(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, ISocketFactory socketFactory, IInvocationStateFactory invocationStateFactory, IPofSerializer pofSerializer) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.socketFactory = socketFactory;
         this.invocationStateFactory = invocationStateFactory;
         this.pofSerializer = pofSerializer;
      }

      public IConnector Create(ITcpEndPoint endpoint) {
         var socket = socketFactory.CreateConnectedSocket(endpoint);
         var context = new ConnectorContext(collectionFactory, threadingProxy, invocationStateFactory);

         // HACK: Send handshake
         pofSerializer.Serialize(socket.GetWriter().__Writer, new X2SHandshake(Role.Client));

         var connector = new Connector(context);
         var reader = new MessageReader(threadingProxy, pofSerializer, context, socket.GetReader());
         var writer = new MessageWriter(threadingProxy, pofSerializer, context, socket.GetWriter());
         reader.Initialize();
         writer.Initialize();
         context.SetReader(reader);
         context.SetWriter(writer);
         return connector;
      }
   }

   public interface IConnectorFactory {
      IConnector Create(ITcpEndPoint endpoint);
   }
}
