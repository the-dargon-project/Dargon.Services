namespace Dargon.Services.Phases.Host {
   public class RemoteInvocationResult {
      public RemoteInvocationResult(bool success, object returnValue) {
         Success = success;
         ReturnValue = returnValue;
      }

      public bool Success { get; private set; }
      public object ReturnValue { get; private set; }
   }
}