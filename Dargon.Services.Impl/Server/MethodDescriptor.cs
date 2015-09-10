using System.Reflection;

namespace Dargon.Services.Server {
   public class MethodDescriptor {
      public MethodDescriptor(MethodInfo methodInfo, int[] outRefParameterIndices) {
         MethodInfo = methodInfo;
         OutRefParameterIndices = outRefParameterIndices;
      }

      public MethodInfo MethodInfo { get; private set; }
      public int[] OutRefParameterIndices { get; private set; }
   }
}