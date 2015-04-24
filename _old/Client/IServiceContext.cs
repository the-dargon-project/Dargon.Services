using System;

namespace Dargon.Services.Client {
   public interface IServiceContext {
      Type ServiceInterface { get; }

      object Invoke(string methodName, object[] methodArguments);
   }
}