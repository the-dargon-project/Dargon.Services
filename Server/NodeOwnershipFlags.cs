using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services.Server {
   [Flags]
   public enum NodeOwnershipFlags {
      Default     = 0,
      HostOnly    = 1,
      GuestOnly   = 2
   }
}
