using System;

namespace Dargon.Services.Client {
   public interface IClientConnector {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}
