using System;
using System.Collections;
using System.Reflection;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;

namespace Dargon.Services.Client {
   public interface InvocationResultTranslator {
      object TranslateOrThrow(object payload, MethodInfo methodInfo, object[] methodArguments);
   }

   public class InvocationResultTranslatorImpl : InvocationResultTranslator {
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public InvocationResultTranslatorImpl(PortableObjectBoxConverter portableObjectBoxConverter) {
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }

      public object TranslateOrThrow(object payload, MethodInfo methodInfo, object[] methodArguments) {
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

         var outRefMethodResult = payload as OutRefMethodResult;
         if (outRefMethodResult != null) {
            payload = outRefMethodResult.ReturnValue;
            var outRefValues = outRefMethodResult.OutRefValues;
            var outRefParameterIndices = methodInfo.GetOutRefParameterIndices();
            var expectedOutRefParameterCount = outRefParameterIndices.Length;
            var actualOutRefParameterCount = outRefValues.Count;
            if (expectedOutRefParameterCount != actualOutRefParameterCount) {
               throw new InvalidOperationException($"Expected {expectedOutRefParameterCount} out/ref parameters but found {actualOutRefParameterCount} in service response.");
            }
            for (var i = 0; i < actualOutRefParameterCount; i++) {
               methodArguments[outRefParameterIndices[i]] = outRefValues[i];
            }
         }

         var exception = payload as Exception;
         if (exception != null) {
            throw exception;
         } else {
            var invocationReturnType = methodInfo.ReturnType;
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