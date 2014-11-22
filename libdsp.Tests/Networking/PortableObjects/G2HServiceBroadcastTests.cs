using ItzWarty.Collections;
using NMockito;
using System;
using Xunit;

namespace Dargon.Services.Networking.PortableObjects {
   public class G2HServiceBroadcastTests : NMockitoInstance {
      private readonly G2HServiceBroadcast testObj;
      private readonly IReadOnlySet<Guid> guids = ImmutableSet.Of(Guid.NewGuid(), Guid.NewGuid());

      public G2HServiceBroadcastTests() {
         testObj = new G2HServiceBroadcast(guids);
      }

      [Fact]
      public void PofSerializationTest() {
         PofTestUtilities.CheckConfiguration(new DspPofContext(), testObj);
      }

      [Fact]
      public void ServiceGuidsReflectsConstructorParameterTest() {
         AssertEquals(guids, testObj.ServiceGuids);
         VerifyNoMoreInteractions();
      }
   }
}
