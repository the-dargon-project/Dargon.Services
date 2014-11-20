using System;
using System.Collections.Generic;
using System.IO;
using Dargon.PortableObjects;
using ItzWarty.Collections;
using NMockito;
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
         var context = new DspPofContext();
         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            serializer.Serialize(ms, testObj);
            ms.Position = 0;
            var deserialized = serializer.Deserialize<G2HServiceBroadcast>(ms);
            AssertEquals(testObj, deserialized);
         }
      }
   }
}
