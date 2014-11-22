using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Services.Networking.PortableObjects {
   public class X2SHandshake : IPortableObject, IEquatable<X2SHandshake> {
      private ClientRole clientRole;

      public X2SHandshake() { }

      public X2SHandshake(ClientRole clientRole) {
         this.clientRole = clientRole;
      }

      public ClientRole ClientRole { get { return clientRole; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU8(0, (byte)clientRole);
      }

      public void Deserialize(IPofReader reader) {
         clientRole = (ClientRole)reader.ReadU8(0);
      }

      public bool Equals(X2SHandshake other) {
         return other.clientRole == this.clientRole;
      }
   }
}
