using System;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class G2HInvocationResult : IPortableObject, IEquatable<G2HInvocationResult> {
      private uint invocationId;
      private object payload;

      public G2HInvocationResult() { }

      public G2HInvocationResult(uint invocationId, PortableException exception) 
         : this(invocationId, (object)exception) { 
      }

      public G2HInvocationResult(uint invocationId, object result) {
         this.invocationId = invocationId;
         this.payload = result;
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteObject(1, payload);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         payload = reader.ReadObject(1);
      }

      public bool Equals(G2HInvocationResult other) {
         return other != null && 
                invocationId == other.invocationId && 
                Equals(payload, other.payload);
      }
   }
}