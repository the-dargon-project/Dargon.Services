using System;
using System.Threading.Tasks;

namespace Dargon.Services.Client {
   public interface IInvocationState {
      uint InvocationId { get; }
      Guid ServiceGuid { get; }
      string MethodName { get; }
      object[] MethodArguments { get; }
      void SetResult(object result);
      void Wait();
      object Result { get; }
   }
}