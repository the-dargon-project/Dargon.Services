using System;
using System.Runtime.InteropServices;

namespace Dargon.Services.Utilities {
   internal static class AttributeUtilities {
      public static Guid GetInterfaceGuid(Type interfaceType) {
         var attribute = (GuidAttribute)interfaceType.GetCustomAttributes(typeof(GuidAttribute), false)[0];
         return Guid.Parse(attribute.Value);
      }
   }
}
