using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Services.Messaging {
   public class OutRefMethodResult : IPortableObject {
      public OutRefMethodResult() { }

      public OutRefMethodResult(object returnValue, IReadOnlyList<object> outRefValues) {
         this.ReturnValue = returnValue;
         this.OutRefValues = outRefValues;
      }

      public object ReturnValue { get; set; }
      public IReadOnlyList<object> OutRefValues { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteObject(0, ReturnValue);
         writer.WriteCollection(1, OutRefValues, true);
      }

      public void Deserialize(IPofReader reader) {
         ReturnValue = reader.ReadObject(0);
         OutRefValues = reader.ReadArray<object>(1, true);
      }
   }
}
