using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services.Phases.Host {
   public interface IHostContext : IDisposable {
      Task<object> Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
      void AddRemoteInvokable(IRemoteInvokable remoteInvokable);
      void RemoveRemoteInvokable(IRemoteInvokable remoteInvokable);
   }

   public interface IRemoteInvokable {
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, object[] arguments);
   }

   public class HostContext : IHostContext {
      public IConcurrentSet<IRemoteInvokable> RemoteInvokables { get; set; }
      private readonly IServiceNodeContext hostServiceNodeContext;
      private readonly IConcurrentSet<IRemoteInvokable> remoteInvokables;

      public HostContext(
         IServiceNodeContext hostServiceNodeContext
      ) : this(
         hostServiceNodeContext, 
         new ConcurrentSet<IRemoteInvokable>()
      ) {}

      public HostContext(IServiceNodeContext hostServiceNodeContext, IConcurrentSet<IRemoteInvokable> remoteInvokables) {
         this.hostServiceNodeContext = hostServiceNodeContext;
         this.remoteInvokables = remoteInvokables;
      }

      public async Task<object> Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         IServiceContext serviceContext;
         if (hostServiceNodeContext.ServiceContextsByGuid.TryGetValue(serviceGuid, out serviceContext)) {
            return serviceContext.HandleInvocation(methodName, methodArguments);
         } else {
            foreach (var remoteInvokable in remoteInvokables) {
               var result = await remoteInvokable.TryRemoteInvoke(serviceGuid, methodName, methodArguments);
               if (result.Success) {
                  return result.ReturnValue;
               }
            }
            throw new ServiceUnavailableException(serviceGuid, methodName);
         }
      }

      public void AddRemoteInvokable(IRemoteInvokable remoteInvokable) {
         remoteInvokables.Add(remoteInvokable);
      }

      public void RemoveRemoteInvokable(IRemoteInvokable remoteInvokable) {
         remoteInvokables.Remove(remoteInvokable);
      }

      public void Dispose() {
      }
   }
}