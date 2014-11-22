using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services.Networking.Server.Sessions {
   public interface IHostSessionFactory {
      IClientSession CreateClientSession(BinaryReader reader, BinaryWriter writer);
      IGuestSession CreateGuestSession(BinaryReader reader, BinaryWriter writer);
   }
}
