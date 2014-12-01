using System;
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
         invocation.ReturnValue = serviceContext.Invoke(methodName, methodArguments);
      }
   }
}