using System;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services.Phases.Host {
   public interface IHostContext : IDisposable {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }

   public class HostContext : IHostContext {
      private readonly IServiceNodeContext hostServiceNodeContext;

      public HostContext(IServiceNodeContext hostServiceNodeContext) {
         this.hostServiceNodeContext = hostServiceNodeContext;
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         IServiceContext serviceContext;
         if (hostServiceNodeContext.ServiceContextsByGuid.TryGetValue(serviceGuid, out serviceContext)) {
            return serviceContext.HandleInvocation(methodName, methodArguments);
         } else {
            //throw new NotImplementedException("TODO: Implement guest invocation.");
//            foreach (var guestSession in guestSessions) {
//               object result;
//               if (guestSession.TryHandleInvocation(serviceGuid, methodName, methodArguments, out result)) {
//               }
//            }
            throw new NotImplementedException();
         }
      }

      public void Dispose() {
      }
   }
}