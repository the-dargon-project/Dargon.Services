using System;

namespace Dargon.Services.Networking.Server.Phases {
   public interface IHostContext {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}