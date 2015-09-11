using System;
using Dargon.PortableObjects;

namespace Dargon.Services.Messaging {
   internal class X2XServiceInvocation : IPortableObject, IEquatable<X2XServiceInvocation> {
      private uint invocationId;
      private Guid serviceGuid;
      private string methodName;
      private PortableObjectBox genericArguments;
      private PortableObjectBox methodArguments;

      public X2XServiceInvocation() { }

      public X2XServiceInvocation(
         uint invocationId, 
         Guid serviceGuid, 
         string methodName,
         PortableObjectBox genericArguments,
         PortableObjectBox methodArguments
      ) {
         this.invocationId = invocationId;
         this.serviceGuid = serviceGuid;
         this.methodName = methodName;
         this.genericArguments = genericArguments;
         this.methodArguments = methodArguments;
      }

      public uint InvocationId { get { return invocationId; } }
      public Guid ServiceGuid { get { return serviceGuid; } }
      public string MethodName { get { return methodName; } }
      public PortableObjectBox GenericArguments { get { return genericArguments; } }
      public PortableObjectBox MethodArguments { get { return methodArguments; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, invocationId);
         writer.WriteGuid(1, serviceGuid);
         writer.WriteString(2, methodName);
         writer.WriteObject(3, genericArguments);
         writer.WriteObject(4, methodArguments);
      }

      public void Deserialize(IPofReader reader) {
         invocationId = reader.ReadU32(0);
         serviceGuid = reader.ReadGuid(1);
         methodName = reader.ReadString(2);
         genericArguments = reader.ReadObject<PortableObjectBox>(3);
         methodArguments = reader.ReadObject<PortableObjectBox>(4);
      }

      public bool Equals(X2XServiceInvocation other) {
         return other != null &&
                invocationId == other.invocationId &&
                serviceGuid.Equals(other.serviceGuid) &&
                methodName.Equals(other.MethodName) &&
                genericArguments.Equals(other.genericArguments) &&
                methodArguments.Equals(other.methodArguments);
      }
   }
}
