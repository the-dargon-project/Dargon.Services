using Dargon.Services.Networking.PortableObjects;

namespace Dargon.Services.Networking.Server.Sessions {
   public interface IHostSession {
      Role Role { get; }

      void Run();
   }
}