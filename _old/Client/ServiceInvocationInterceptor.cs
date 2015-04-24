using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Dargon.Services.PortableObjects;
using Dargon.Services.Utilities;

namespace Dargon.Services.Client {
   public class ServiceInvocationInterceptor : IInterceptor {
      private readonly IServiceContext serviceContext;

      public ServiceInvocationInterceptor(IServiceContext serviceContext) {
         this.serviceContext = serviceContext;
      }

      public void Intercept(IInvocation invocation) {
         var methodName = invocation.Method.Name;
         var methodArguments = invocation.Arguments;
         var result = serviceContext.Invoke(methodName, methodArguments);
         var exception = result as Exception;
         if (exception != null) {
            throw exception;
         } else {
            var returnType = invocation.Method.ReturnType;
            if (returnType.IsArray) {
               var array = (Array)Activator.CreateInstance(returnType, ((Array)result).Length);
               Array.Copy((Array)result, array, array.Length);
               result = array;
            } else if (typeof(IEnumerable).IsAssignableFrom(returnType) && returnType != typeof(string)) {
               var elementType = returnType.GetGenericArguments()[0];
               var array = Array.CreateInstance(elementType, ((Array)result).Length);
               Array.Copy((Array)result, array, array.Length);
               result = array;
            }
            invocation.ReturnValue = result;;
         }
      }
   }
}