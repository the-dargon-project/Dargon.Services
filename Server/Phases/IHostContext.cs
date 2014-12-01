using System;

namespace Dargon.Services.Server.Phases {
   public interface IHostContext : IDisposable {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
   }

   public class HostContext : IHostContext {
      private readonly IConnectorContext hostConnectorContext;

      public HostContext(IConnectorContext hostConnectorContext) {
         this.hostConnectorContext = hostConnectorContext;
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         IServiceContext serviceContext;
         if (hostConnectorContext.ServiceContextsByGuid.TryGetValue(serviceGuid, out serviceContext)) {
            return serviceContext.HandleInvocation(methodName, methodArguments);
         } else {
            throw new NotImplementedException("TODO: Implement guest invocation.");
         }
      }

      public void Dispose() {
      }
   }
}