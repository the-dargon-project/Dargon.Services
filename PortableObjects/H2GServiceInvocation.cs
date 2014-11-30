using System;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class H2GServiceInvocation : IPortableObject {
      private uint invocationId;
      private Guid serviceGuid;
      private string methodName;
      private object[] methodArguments;

      public H2GServiceInvocation() { }

      public H2GServiceInvocation(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         this.invocationId = invocationId;
         this.serviceGuid = serviceGuid;
         this.methodName = methodName;
         this.methodArguments = methodArguments;
      }

      public uint InvocationId { get { return invocationId; } }
      public Guid ServiceGuid { get { return serviceGuid; } }
      public string MethodName { get { return methodName; } }
      public object[] MethodArguments { get { return methodArguments; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteGuid(1, serviceGuid);
         writer.WriteString(2, methodName);
         writer.WriteCollection(3, methodArguments, true);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         serviceGuid = reader.ReadGuid(1);
         methodName = reader.ReadString(2);
         methodArguments = reader.ReadArray<object>(3, true);
      }
   }
}
