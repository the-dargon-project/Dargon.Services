using System;

namespace Dargon.Services.Client {
   public interface IServiceClient : IDisposable {
      TService GetService<TService>() where TService : class;
   }
}
