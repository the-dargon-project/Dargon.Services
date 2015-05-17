using System;
using Dargon.PortableObjects;

namespace Dargon.Services.Messaging {
   internal class X2XInvocationResult : IPortableObject, IEquatable<X2XInvocationResult> {
      private uint invocationId;
      private PortableObjectBox payloadBox;

      public X2XInvocationResult() { }

      public X2XInvocationResult(uint invocationId, PortableObjectBox payloadBox) {
         this.invocationId = invocationId;
         this.payloadBox = payloadBox;
      }

      public uint InvocationId { get { return invocationId; } }
      public PortableObjectBox PayloadBox { get { return payloadBox; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteObject(1, payloadBox);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         payloadBox = reader.ReadObject<PortableObjectBox>(1);
      }

      public bool Equals(X2XInvocationResult other) {
         return other != null &&
                invocationId == other.invocationId &&
                Equals(payloadBox, other.payloadBox);
      }
   }
}