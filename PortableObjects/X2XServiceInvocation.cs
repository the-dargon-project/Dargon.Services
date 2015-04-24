using System;
using System.Linq;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   internal class X2XServiceInvocation : IPortableObject, IEquatable<X2XServiceInvocation> {
      private uint invocationId;
      private Guid serviceGuid;
      private string methodName;
      private object[] methodArguments;

      public X2XServiceInvocation() { }

      public X2XServiceInvocation(
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

      public bool Equals(X2XServiceInvocation other) { 
         return other != null && 
                invocationId == other.invocationId && 
                serviceGuid.Equals(other.serviceGuid) && 
                methodName.Equals(other.MethodName) && 
                methodArguments.SequenceEqual(other.methodArguments); 
      }
   }
}
