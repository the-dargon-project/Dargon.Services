using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class DspPofContext : PofContext {
      public DspPofContext() {
         RegisterPortableObjectType(1, typeof(G2HServiceBroadcast));
         RegisterPortableObjectType(2, typeof(X2SHandshake));
         RegisterPortableObjectType(3, typeof(C2HServiceInvocation));
      }
   }
}
