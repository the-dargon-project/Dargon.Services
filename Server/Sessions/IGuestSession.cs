using System;

namespace Dargon.Services.Server.Sessions {
   public interface IGuestSession : IHostSession {
      bool TryHandleInvocation(Guid serviceGuid, string methodName, object[] methodArguments, out object result);
   }
}