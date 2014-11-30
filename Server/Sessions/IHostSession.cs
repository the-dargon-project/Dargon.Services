using Dargon.Services.PortableObjects;

namespace Dargon.Services.Server.Sessions {
   public interface IHostSession {
      Role Role { get; }

      void Run();
   }
}