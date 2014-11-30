using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services.Utilities {
   internal static class AttributeUtilities {
      public static Guid GetInterfaceGuid(Type interfaceType) {
         var attribute = (GuidAttribute)interfaceType.GetCustomAttributes(typeof(GuidAttribute), false)[0];
         return Guid.Parse(attribute.Value);
      }
   }
}
