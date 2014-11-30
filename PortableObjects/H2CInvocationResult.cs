using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class H2CInvocationResult : IPortableObject {
      private uint invocationId;
      private object payload;

      public H2CInvocationResult() { }

      public H2CInvocationResult(uint invocationId, object payload) {
         this.invocationId = invocationId;
         this.payload = payload;
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteObject(1, payload);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         payload = reader.ReadObject<IPortableObject>(1);
      }
   }
}