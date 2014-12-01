using System;
using System.Threading;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public class InvocationState : IInvocationState {
      private readonly uint invocationId;
      private readonly Guid serviceGuid;
      private readonly string methodName;
      private readonly object[] methodArguments;
      private readonly ICountdownEvent resultSetSignal;
      private object result;

      public InvocationState(IThreadingProxy threadingProxy, uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         this.invocationId = invocationId;
         this.serviceGuid = serviceGuid;
         this.methodName = methodName;
         this.methodArguments = methodArguments;
         this.resultSetSignal = threadingProxy.CreateCountdownEvent(1);
         this.result = null;
      }

      public uint InvocationId { get { return invocationId; } }
      public Guid ServiceGuid { get { return serviceGuid; } }
      public string MethodName { get { return methodName; } }
      public object[] MethodArguments { get { return methodArguments; } }
      public object Result { get { return result; } }

      public void SetResult(object result) {
         this.result = result;
         this.resultSetSignal.Signal();
      }

      public void Wait() {
         resultSetSignal.Wait();
      }
   }
}