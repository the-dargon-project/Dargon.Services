using System;

namespace Dargon.Services.Server.Phases {
   public interface IHostContext {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}