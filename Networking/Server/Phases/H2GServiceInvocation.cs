using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Services.Networking.Server.Phases {
   public class H2GServiceInvocation : IPortableObject {
      private Guid serviceGuid;
      private string methodName;
      private object[] methodArguments;

      public H2GServiceInvocation(Guid serviceGuid, string methodName, object[] methodArguments) {
         this.serviceGuid = serviceGuid;
         this.methodName = methodName;
         this.methodArguments = methodArguments;
      }

      public Guid ServiceGuid { get { return serviceGuid; } }
      public string MethodName { get { return methodName; } }
      public object[] MethodArguments { get { return methodArguments; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteGuid(0, serviceGuid);
         writer.WriteString(1, methodName);
         writer.WriteCollection(2, methodArguments, true);
      }

      public void Deserialize(IPofReader reader) {
         serviceGuid = reader.ReadGuid(0);
         methodName = reader.ReadString(1);
         methodArguments = reader.ReadArray<object>(2, true);
      }
   }
}
