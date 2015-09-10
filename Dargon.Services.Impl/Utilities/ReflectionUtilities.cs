using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services.Utilities {
   public static class ReflectionUtilities {
      public static int[] GetOutRefParameterIndices(this MethodInfo methodInfo) {
         var methodParameters = methodInfo.GetParameters();
         var outRefParameterIndices = Enumerable.Range(0, methodParameters.Length)
            .Where(parameterIndex => methodParameters[parameterIndex].IsOut ||
                                     methodParameters[parameterIndex].ParameterType.IsByRef)
            .ToArray();
         return outRefParameterIndices;
      }
   }
}
