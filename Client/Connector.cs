using System;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public class Connector : IConnector{
      private readonly IUserConnectorContext userConnectorContext;

      public Connector(IUserConnectorContext userConnectorContext) {
         this.userConnectorContext = userConnectorContext;
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         return userConnectorContext.Invoke(serviceGuid, methodName, methodArguments);
      }
   }

   public interface IConnectorInternalFactory {
   }
}