using System;
using System.Reflection;
using Castle.DynamicProxy;
using ItzWarty;

namespace Dargon.Services.Utilities {
   public static class AsyncInterceptorUtilities {
      public static bool TryGetAsyncInterceptor(object instance, out IAsyncInterceptor asyncInterceptor) {
         var instanceTypeInfo = instance.GetType().GetTypeInfo();
         if (instanceTypeInfo.FullName.Contains("Castle.Proxies")) {
            var interceptors = (IInterceptor[])instanceTypeInfo.GetField("__interceptors").GetValue(instance);
            if (interceptors.Length != 1) {
               throw new InvalidOperationException("Encountered multiple interceptors for type " + instanceTypeInfo.FullName + ": " + interceptors.Join(", "));
            }
            
            asyncInterceptor = (IAsyncInterceptor)interceptors[0];
            return true;
         } else {
            asyncInterceptor = null;
            return false;
         }
      }
   }
}
