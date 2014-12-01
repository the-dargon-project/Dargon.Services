using System;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public class Connector : IConnector{
      private readonly IUserConnectorContext context;

      public Connector(IUserConnectorContext context) {
         this.context = context;
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         return context.Invoke(serviceGuid, methodName, methodArguments);
      }

      public void Dispose() {
         context.Dispose();
      }
   }

   public interface IConnectorInternalFactory {
   }
}