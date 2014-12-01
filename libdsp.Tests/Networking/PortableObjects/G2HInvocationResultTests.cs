using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.PortableObjects;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.PortableObjects {
   public class G2HInvocationResultTests : NMockitoInstance {
      private readonly G2HInvocationResult testObj;
      private readonly uint invocationId = 1337;
      private readonly PortableException exception = new PortableException("Exception Type", "Exception Message", "Stack Trace", null);

      public G2HInvocationResultTests() {
         testObj = new G2HInvocationResult(invocationId, exception);
      }

      [Fact]
      public void PofSerializationTest() {
         PofTestUtilities.CheckConfiguration(new DspPofContext(), testObj);
      }
   }
}
