using System;

namespace Dargon.Services.Client {
   public interface IConnector {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}
