using System;

namespace Dargon.Services {
   [Flags]
   public enum ClusteringRoleFlags {
      Default     = 0,
      HostOnly    = 1,
      GuestOnly   = 2
   }
}
