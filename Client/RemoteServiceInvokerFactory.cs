using System;

namespace Dargon.Services.Client {
   public interface RemoteServiceInvokerFactory {
      RemoteServiceInvocationValidator Create(Type serviceInterface);
   }
}