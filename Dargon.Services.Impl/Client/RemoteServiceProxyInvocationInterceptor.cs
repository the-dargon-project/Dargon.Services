using Dargon.Services.Clustering.Local;
using Dargon.Services.Utilities;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dargon.Services.Client {
   public class RemoteServiceProxyInvocationInterceptor : AsyncInterceptorBase {
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

      public override async Task<object> InterceptAsync(MethodInfo methodInfo, object[] methodArguments) {
         var methodName = methodInfo.Name;
         var genericArguments = methodInfo.GetGenericArguments();

         validator.ValidateInvocationOrThrow(methodName, genericArguments, methodArguments);

         var payload = await clusteringPhaseManager.InvokeServiceCall(serviceGuid, methodName, genericArguments, methodArguments);
         return translator.TranslateOrThrow(payload, methodInfo, methodArguments);
      }
   }
}