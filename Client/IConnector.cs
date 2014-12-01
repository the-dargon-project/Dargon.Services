using System;

namespace Dargon.Services.Client {
   public interface IConnector : IDisposable {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}
