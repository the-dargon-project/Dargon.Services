using System;
using Dargon.Services.Server.Sessions;
using ItzWarty.Collections;

namespace Dargon.Services.Server.Phases {
   public interface IHostContext : IDisposable {
      object Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
      void AddGuestSession(IGuestSession guestSession);
      void RemoveGuestSessions(IGuestSession guestSession);
   }

   public class HostContext : IHostContext {
      private readonly IConnectorContext hostConnectorContext;
      private readonly IConcurrentSet<IGuestSession> guestSessions;

      public HostContext(IConnectorContext hostConnectorContext) 
      : this(hostConnectorContext, new ConcurrentSet<IGuestSession>()) {
      }

      public HostContext(IConnectorContext hostConnectorContext, IConcurrentSet<IGuestSession> guestSessions) {
         this.hostConnectorContext = hostConnectorContext;
         this.guestSessions = guestSessions;
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         IServiceContext serviceContext;
         if (hostConnectorContext.ServiceContextsByGuid.TryGetValue(serviceGuid, out serviceContext)) {
            return serviceContext.HandleInvocation(methodName, methodArguments);
         } else {
            //throw new NotImplementedException("TODO: Implement guest invocation.");
            foreach (var guestSession in guestSessions) {
               object result;
               if (guestSession.TryHandleInvocation(serviceGuid, methodName, methodArguments, out result)) {
               }
            }
            throw new NotImplementedException();
         }
      }

      public void AddGuestSession(IGuestSession guestSession) {
         guestSessions.Add(guestSession);
      }

      public void RemoveGuestSessions(IGuestSession guestSession) {
         guestSessions.Remove(guestSession);
      }

      public void Dispose() {
      }
   }
}