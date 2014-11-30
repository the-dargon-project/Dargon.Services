using System;

namespace Dargon.Services.Server {
   public interface IServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, object[] arguments);
   }
}
