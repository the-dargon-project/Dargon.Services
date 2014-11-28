using Dargon.PortableObjects;
using System;
using System.Linq;

namespace Dargon.Services.Networking.PortableObjects {
   public class C2HServiceInvocation : IPortableObject, IEquatable<C2HServiceInvocation> {
      private uint invocationId;
      private Guid serviceGuid;
      private string methodName;
      private object[] methodArguments;

      public C2HServiceInvocation() { }

      public C2HServiceInvocation(
         uint invocationId, 
         Guid serviceGuid, 
         string methodName, 
         object[] methodArguments
      ) {
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

      public bool Equals(C2HServiceInvocation other) { 
         return other != null && 
                invocationId == other.invocationId && 
                serviceGuid.Equals(other.serviceGuid) && 
                methodName.Equals(other.MethodName) && 
                methodArguments.SequenceEqual(other.methodArguments); 
      }
   }
}
