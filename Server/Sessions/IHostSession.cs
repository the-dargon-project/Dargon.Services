using System;
using Dargon.Services.PortableObjects;

namespace Dargon.Services.Server.Sessions {
   public interface IHostSession : IDisposable {
      Role Role { get; }

      void Run();
   }
}