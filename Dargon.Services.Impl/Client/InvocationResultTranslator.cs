using System;
using System.Collections;
using Dargon.Services.Messaging;

namespace Dargon.Services.Client {
   public interface InvocationResultTranslator {
      object TranslateOrThrow(object payload, Type invocationReturnType);
   }

   public class InvocationResultTranslatorImpl : InvocationResultTranslator {
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public InvocationResultTranslatorImpl(PortableObjectBoxConverter portableObjectBoxConverter) {
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }

      public object TranslateOrThrow(object payload, Type invocationReturnType) {
         var portableObjectBox = payload as PortableObjectBox;
         if (portableObjectBox != null) {
            object[] conversion;
            portableObjectBoxConverter.TryConvertFromDataTransferObject(portableObjectBox, out conversion);
            if (conversion.Length != 1) {
               throw new InvalidOperationException("Expected translated POB to have length 1 but found length " + conversion.Length);
            } else {
               payload = conversion[0];
            }
         }

         var exception = payload as Exception;
         if (exception != null) {
            throw exception;
         } else {
            if (invocationReturnType.IsArray) {
               var array = (Array)Activator.CreateInstance(invocationReturnType, ((Array)payload).Length);
               Array.Copy((Array)payload, array, array.Length);
               payload = array;
            } else if (typeof(IEnumerable).IsAssignableFrom(invocationReturnType) && invocationReturnType != typeof(string)) {
               var elementType = invocationReturnType.GetGenericArguments()[0];
               var array = Array.CreateInstance(elementType, ((Array)payload).Length);
               Array.Copy((Array)payload, array, array.Length);
               payload = array;
            }
            return payload;
         }
      }
   }
}