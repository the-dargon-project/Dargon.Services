using System;
using Dargon.PortableObjects;

namespace Dargon.Services.Networking.PortableObjects {
   public class H2CInvocationResult : IPortableObject {
      private uint invocationId;
      private object payload;

      public H2CInvocationResult() { }

      public H2CInvocationResult(uint invocationId, PortableException exception) : this(invocationId, (object)exception) { }

      public H2CInvocationResult(uint invocationId, object result) {
         this.invocationId = invocationId;
         this.payload = result;
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteObject(1, (IPortableObject)payload);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         payload = reader.ReadObject<IPortableObject>(1);
      }
   }
}