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

      public PortableObjectBox ConvertToDataTransferObject<T>(T methodArguments) {
         using (var outerMs = streamFactory.CreateMemoryStream()) {
            pofSerializer.Serialize(outerMs.Writer, methodArguments);
            return new PortableObjectBox(outerMs.GetBuffer(), 0, (int)outerMs.Length);
         }
      }

      public bool TryConvertFromDataTransferObject<T>(PortableObjectBox dto, out T methodArguments) {
         using (var ms = streamFactory.CreateMemoryStream(dto.Buffer, dto.Offset, dto.Length)) {
            try {
               methodArguments = (T)pofSerializer.Deserialize(ms.Reader);
               return true;
            } catch (TypeNotFoundException) {
               methodArguments = default(T);
               return false;
            }
         }
      }
   }
}
