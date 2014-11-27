using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
using Dargon.Services.Networking.Server.Phases;
using ItzWarty.Collections;
using ItzWarty.IO;

namespace Dargon.Services.Networking.Server.Sessions {
   public class HostSessionBase : IHostSession {
      protected readonly ICollectionFactory collectionFactory;
      protected readonly IPofSerializer pofSerializer;
      protected readonly IHostContext hostContext;
      protected readonly IBinaryReader reader;
      protected readonly IBinaryWriter writer;
      private readonly Role role;
      private readonly IDictionary<Type, Action<IPortableObject>> handlers;

      protected HostSessionBase(ICollectionFactory collectionFactory, IPofSerializer pofSerializer, IHostContext hostContext, IBinaryReader reader, IBinaryWriter writer, Role role) {
         this.collectionFactory = collectionFactory;
         this.pofSerializer = pofSerializer;
         this.hostContext = hostContext;
         this.reader = reader;
         this.writer = writer;
         this.role = role;

         this.handlers = collectionFactory.CreateDictionary<Type, Action<IPortableObject>>();
      }

      public Role Role { get { return role; } }
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
