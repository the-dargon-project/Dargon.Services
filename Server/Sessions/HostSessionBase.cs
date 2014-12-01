using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Server.Sessions {
   public abstract class HostSessionBase : IHostSession {
      protected readonly ICollectionFactory collectionFactory;
      protected readonly IPofSerializer pofSerializer;
      protected readonly IHostContext hostContext;
      protected readonly IConnectedSocket socket;
      protected readonly IBinaryReader reader;
      protected readonly IBinaryWriter writer;
      private readonly IThread thread;
      private readonly IDictionary<Type, Action<IPortableObject>> handlers;

      protected HostSessionBase(
         ICollectionFactory collectionFactory, 
         IPofSerializer pofSerializer, 
         IHostContext hostContext, 
         IConnectedSocket socket,
         IThread thread
      ) {
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
         this.hostContext = hostContext;
         this.socket = socket;
         this.thread = thread;

         this.handlers = collectionFactory.CreateDictionary<Type, Action<IPortableObject>>();
         this.reader = socket.GetReader();
         this.writer = socket.GetWriter();
      }

      public abstract Role Role { get; }

      public void Run() {
         Debug.WriteLine("HostSessionBase Run()");
         while (true) {
            var message = pofSerializer.Deserialize<IPortableObject>(reader.__Reader);
            var messageType = message.GetType();
            Debug.WriteLine("Received message of type " + messageType);
            handlers[messageType](message);
         }
      }

      protected void RegisterMessageHandler<T>(Action<T> handler) where T : IPortableObject {
         RegisterMessageHandler(typeof(T), portableObject => handler((T)portableObject));
      }

      protected void RegisterMessageHandler(Type messageType, Action<IPortableObject> handler) {
         handlers.Add(messageType, handler);
      }

      public void Dispose() {
         this.reader.Dispose();
         this.writer.Dispose();
         this.socket.Dispose();
         this.thread.Join();
      }
   }
}
