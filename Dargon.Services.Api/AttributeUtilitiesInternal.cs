using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dargon.Services {
   internal static class AttributeUtilitiesInternal {
      public static bool TryGetInterfaceGuid(Type interfaceType, out Guid guid) {
         var typeInfo = interfaceType.GetTypeInfo();
         var guidAttribute = typeInfo.GetCustomAttribute<GuidAttribute>();
         if (guidAttribute == null) {
            guid = Guid.Empty;
            return false;
         } else {
            guid = Guid.Parse(guidAttribute.Value);
            return true;
         }
      }
   }
}
