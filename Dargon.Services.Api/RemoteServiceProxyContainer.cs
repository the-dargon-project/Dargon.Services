using System;

namespace Dargon.Services {
   public interface RemoteServiceProxyContainer {
      TService GetService<TService>() where TService : class;
      TService GetService<TService>(Guid serviceGuid) where TService : class;
   }
}