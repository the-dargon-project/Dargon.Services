using Dargon.PortableObjects;

namespace Dargon.Services.Messaging {
   public class DspPofContext : PofContext {
      public const int kPofIdentifierOffset = 0;

      public DspPofContext() {
         RegisterPortableObjectType(kPofIdentifierOffset + 1, typeof(G2HServiceBroadcast));
//         RegisterPortableObjectType(kPofIdentifierOffset + 2, typeof(X2SHandshake));
         RegisterPortableObjectType(kPofIdentifierOffset + 3, typeof(X2XServiceInvocation));
         RegisterPortableObjectType(kPofIdentifierOffset + 4, typeof(X2XInvocationResult));
         RegisterPortableObjectType(kPofIdentifierOffset + 5, typeof(PortableException));
         RegisterPortableObjectType(kPofIdentifierOffset + 6, typeof(G2HServiceUpdate));
         RegisterPortableObjectType(kPofIdentifierOffset + 7, typeof(PortableObjectBox));
         RegisterPortableObjectType(kPofIdentifierOffset + 8, typeof(OutRefMethodResult));
      }
   }
}
