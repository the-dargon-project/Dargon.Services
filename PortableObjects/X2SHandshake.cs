using System;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class X2SHandshake : IPortableObject, IEquatable<X2SHandshake> {
      private Role role;

      public X2SHandshake() { }

      public X2SHandshake(Role role) {
         this.role = role;
      }

      public Role Role { get { return role; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU8(0, (byte)role);
      }

      public void Deserialize(IPofReader reader) {
         role = (Role)reader.ReadU8(0);
      }

      public bool Equals(X2SHandshake other) {
         return other.role == this.role;
      }
   }
}
