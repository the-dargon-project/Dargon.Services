using System;
using ItzWarty;

namespace Dargon.Services {
   public class ServiceUnavailableException : Exception {
      public ServiceUnavailableException(
         Guid serviceGuid, 
         string methodName
      ) : base(GenerateErrorMessage(serviceGuid, methodName)) {
      }

      public static string GenerateErrorMessage(Guid serviceGuid, string methodName) {
         return "Service of guid {0} unavailable (attempting to invoke method {1}).".F(serviceGuid, methodName);
      }
   }
}
