using Castle.DynamicProxy;
using System;
using System.Collections;

namespace Dargon.Services.Client {
   public interface InvocationResultTranslator {
      object TranslateOrThrow(object payload, Type invocationReturnType);
   }

   public class InvocationResultTranslatorImpl : InvocationResultTranslator {
      public object TranslateOrThrow(object payload, Type invocationReturnType) {
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