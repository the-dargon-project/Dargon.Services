using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using ItzWarty.IO;

namespace Dargon.Services.Messaging {
   public class PortableObjectBoxConverter {
      private readonly IStreamFactory streamFactory;
      private readonly IPofSerializer pofSerializer;

      public PortableObjectBoxConverter(IStreamFactory streamFactory, IPofSerializer pofSerializer) {
         this.streamFactory = streamFactory;
         this.pofSerializer = pofSerializer;
      }

      public PortableObjectBox ConvertToDataTransferObject(object[] methodArguments) {
         using (var outerMs = streamFactory.CreateMemoryStream()) {
            pofSerializer.Serialize(outerMs.Writer, (object)methodArguments);
            return new PortableObjectBox(outerMs.GetBuffer(), 0, (int)outerMs.Length);
         }
      }

      public bool TryConvertFromDataTransferObject(PortableObjectBox dto, out object[] methodArguments) {
         using (var ms = streamFactory.CreateMemoryStream(dto.Buffer, dto.Offset, dto.Length)) {
            try {
               methodArguments = (object[])pofSerializer.Deserialize(ms.Reader);
               return true;
            } catch (TypeNotFoundException) {
               methodArguments = null;
               return false;
            }
         }
      }
   }
}
