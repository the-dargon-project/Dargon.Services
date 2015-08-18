using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Dargon.Services.Utilities {
   internal static class AttributeUtilities {
      private static readonly AttributeUtilitiesInterface instance = new AttributeUtilitiesImpl();

      public static bool TryGetInterfaceGuid(Type interfaceType, out Guid guid) {
         return instance.TryGetInterfaceGuid(interfaceType, out guid);
      }
   }

   public interface AttributeUtilitiesInterface {
      bool TryGetInterfaceGuid(Type interfaceType, out Guid guid);
   }

   public class AttributeUtilitiesImpl : AttributeUtilitiesInterface {
      public bool TryGetInterfaceGuid(Type interfaceType, out Guid guid) {
         var attributes = interfaceType.GetCustomAttributes(typeof(GuidAttribute), false);
         if (attributes.Any()) {
            guid = Guid.Parse(((GuidAttribute)attributes.First()).Value);
            return true;
         } else {
            guid = Guid.Empty;
            return false;
         }
      }
   }
}
