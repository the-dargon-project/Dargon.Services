using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Dargon.Services.Utilities;

namespace Dargon.Services.Clustering.Remote {
   public class LoadBalancedServiceProxyInterceptorImpl<TService> : AsyncInterceptorBase where TService : class {
      private readonly object updateSynchronization = new object();
      private readonly RemoteServiceClientSource remoteServiceClientsSource;
      private readonly Guid serviceGuid;
      private ServiceClient[] previousServiceClients = null;
      private TService[] services = null;
      private int counter = 0;

      public LoadBalancedServiceProxyInterceptorImpl(RemoteServiceClientSource remoteServiceClientsSource, Guid serviceGuid) {
         this.remoteServiceClientsSource = remoteServiceClientsSource;
         this.serviceGuid = serviceGuid;
      }

      public override Task<object> InterceptAsync(MethodInfo methodInfo, object[] methodArguments) {
         SynchronizeServices();

         var count = Interlocked.Increment(ref counter);
         var candidates = services;
         var candidate = candidates[count % candidates.Length];

         IAsyncInterceptor asyncInterceptor;
         if (AsyncInterceptorUtilities.TryGetAsyncInterceptor(candidate, out asyncInterceptor)) {
            return asyncInterceptor.InterceptAsync(methodInfo, methodArguments);
         } else {
            return Task.FromResult(methodInfo.Invoke(candidate, methodArguments));
         }
      }

      private void SynchronizeServices() {
         var currentServiceClients = remoteServiceClientsSource.ServiceClients;
         if (currentServiceClients != previousServiceClients) {
            lock (updateSynchronization) {
               currentServiceClients = remoteServiceClientsSource.ServiceClients;
               if (currentServiceClients != previousServiceClients) {
                  previousServiceClients = currentServiceClients;
                  services = currentServiceClients.Select(x => x.GetService<TService>()).ToArray();
               }
            }
         }
      }
   }
}