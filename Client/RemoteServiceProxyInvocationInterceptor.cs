using Castle.DynamicProxy;
using Dargon.Services.Clustering;
using System;

namespace Dargon.Services.Client {
   public class RemoteServiceProxyInvocationInterceptor : IInterceptor {
      private readonly Guid serviceGuid;
      private readonly RemoteServiceInvocationValidator validator;
      private readonly InvocationResultTranslator translator;
      private readonly ClusteringPhaseManager clusteringPhaseManager;

      public RemoteServiceProxyInvocationInterceptor(Guid serviceGuid, RemoteServiceInvocationValidator validator, InvocationResultTranslator translator, ClusteringPhaseManager clusteringPhaseManager) {
         this.serviceGuid = serviceGuid;
         this.validator = validator;
         this.translator = translator;
         this.clusteringPhaseManager = clusteringPhaseManager;
      }

      public void Intercept(IInvocation invocation) {
         var methodName = invocation.Method.Name;
         var methodArguments = invocation.Arguments;

         validator.ValidateInvocationOrThrow(methodName, methodArguments);

         var payload = clusteringPhaseManager.InvokeServiceCall(serviceGuid, methodName, methodArguments).Result;
         invocation.ReturnValue = translator.TranslateOrThrow(payload, invocation.Method.ReturnType);
      }
   }
}