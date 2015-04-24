using System;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   internal class X2XInvocationResult : IPortableObject, IEquatable<X2XInvocationResult> {
      private uint invocationId;
      private object payload;

      public X2XInvocationResult() { }

      public X2XInvocationResult(uint invocationId, object payload) {
         this.invocationId = invocationId;
         this.payload = payload;
      }

      public uint InvocationId { get { return invocationId; } }
      public object Payload { get { return payload; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteObject(1, payload);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         payload = reader.ReadObject(1);
      }

      public bool Equals(X2XInvocationResult other) {
         return other != null &&
                invocationId == other.invocationId &&
                Equals(payload, other.payload);
      }
   }
}