using System;
using System.Collections.Generic;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Server.Sessions {
   public abstract class HostSessionBase : IHostSession {
      protected readonly ICollectionFactory collectionFactory;
      protected readonly IPofSerializer pofSerializer;
      protected readonly IHostContext hostContext;
      protected readonly IBinaryReader reader;
      protected readonly IBinaryWriter writer;
      private readonly IDictionary<Type, Action<IPortableObject>> handlers;

      protected HostSessionBase(
         ICollectionFactory collectionFactory, 
         IPofSerializer pofSerializer, 
         IHostContext hostContext, 
         IBinaryReader reader, 
         IBinaryWriter writer
      ) {
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
         this.hostContext = hostContext;
         this.reader = reader;
         this.writer = writer;

         this.handlers = collectionFactory.CreateDictionary<Type, Action<IPortableObject>>();
      }

      public abstract Role Role { get; }

      public void Run() {
         while (true) {
            var message = pofSerializer.Deserialize<IPortableObject>(reader.__Reader);
            var messageType = message.GetType();
            handlers[messageType](message);
         }
      }

      protected void RegisterMessageHandler<T>(Action<T> handler) where T : IPortableObject {
         RegisterMessageHandler(typeof(T), portableObject => handler((T)portableObject));
      }

      protected void RegisterMessageHandler(Type messageType, Action<IPortableObject> handler) {
         handlers.Add(messageType, handler);
      }
   }
}
