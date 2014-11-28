using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Services.Networking.PortableObjects {
   public class DspPofContext : PofContext {
      public DspPofContext() {
         RegisterPortableObjectType(1, typeof(G2HServiceBroadcast));
         RegisterPortableObjectType(2, typeof(X2SHandshake));
         RegisterPortableObjectType(3, typeof(C2HServiceInvocation));
      }
   }
}
