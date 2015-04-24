using System;
using Dargon.Services.Client;
using ItzWarty.Threading;

namespace Dargon.Services.Common {
   public interface IInvocationStateFactory {
      IInvocationState Create(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments);
   }

   public class InvocationStateFactory : IInvocationStateFactory {
      private readonly IThreadingProxy threadingProxy;

      public InvocationStateFactory(IThreadingProxy threadingProxy) {
         this.threadingProxy = threadingProxy;
      }

      public IInvocationState Create(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         return new InvocationState(threadingProxy, invocationId, serviceGuid, methodName, methodArguments);
      }
   }
}