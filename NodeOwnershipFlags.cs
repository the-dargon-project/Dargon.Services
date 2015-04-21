using System;

namespace Dargon.Services {
   [Flags]
   public enum NodeOwnershipFlags {
      Default     = 0,
      HostOnly    = 1,
      GuestOnly   = 2
   }
}
