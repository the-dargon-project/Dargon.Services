using Castle.DynamicProxy;
using Dargon.Services.Clustering;
using System;
using System.Reflection;
using System.Threading.Tasks;

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
         invocation.ReturnValue = RunInterceptedInvocationAsync(invocation.Method, invocation.Arguments).Result;
      }

      public async Task<object> RunInterceptedInvocationAsync(MethodInfo methodInfo, object[] methodArguments) {
         var methodName = methodInfo.Name;
         var returnType = methodInfo.ReturnType;

         validator.ValidateInvocationOrThrow(methodName, methodArguments);

         var payload = await clusteringPhaseManager.InvokeServiceCall(serviceGuid, methodName, methodArguments);
         return translator.TranslateOrThrow(payload, returnType);
      }
   }
}