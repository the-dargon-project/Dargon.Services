using System.IO;

namespace Dargon.Services.Server.Sessions {
   public interface IHostSessionFactory {
      IClientSession CreateClientSession(BinaryReader reader, BinaryWriter writer);
      IGuestSession CreateGuestSession(BinaryReader reader, BinaryWriter writer);
   }
}
