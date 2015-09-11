using System;

namespace Dargon.Services {
   public interface RemoteServiceProxyContainer {
      TService GetService<TService>(Guid serviceGuid) where TService : class;
   }
}