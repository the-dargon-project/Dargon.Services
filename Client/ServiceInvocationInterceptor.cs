using System;
using Castle.DynamicProxy;
using Dargon.Services.PortableObjects;
using Dargon.Services.Utilities;

namespace Dargon.Services.Client {
   public class ServiceInvocationInterceptor : IInterceptor {
      private readonly Type serviceInterface;
      private readonly IServiceContext serviceContext;

      public ServiceInvocationInterceptor(Type serviceInterface, IServiceContext serviceContext) {
         this.serviceInterface = serviceInterface;
         this.serviceContext = serviceContext;
      }

      public void Intercept(IInvocation invocation) {
         var serviceGuid = clientConnector.ServiceGuid;
         var methodName = invocation.Method.Name;
         var methodArguments = invocation.Arguments;
         var dto = new C2HServiceInvocation(invocationId, serviceGuid, methodName, methodArguments);
      }
   }
}