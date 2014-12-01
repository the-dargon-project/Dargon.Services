using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class DspPofContext : PofContext {
      public const int kPofIdentifierOffset = 0;

      public DspPofContext() {
         RegisterPortableObjectType(kPofIdentifierOffset + 1, typeof(G2HServiceBroadcast));
         RegisterPortableObjectType(kPofIdentifierOffset + 2, typeof(X2SHandshake));
         RegisterPortableObjectType(kPofIdentifierOffset + 3, typeof(C2HServiceInvocation));
         RegisterPortableObjectType(kPofIdentifierOffset + 4, typeof(H2CInvocationResult));
         RegisterPortableObjectType(kPofIdentifierOffset + 5, typeof(PortableException));
         RegisterPortableObjectType(kPofIdentifierOffset + 6, typeof(G2HInvocationResult));
      }
   }
}
