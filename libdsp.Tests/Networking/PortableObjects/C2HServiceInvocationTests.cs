using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.PortableObjects;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.PortableObjects {
   public class C2HServiceInvocationTests : NMockitoInstance {
      private readonly X2XServiceInvocation testObj;
      private readonly uint invocationId = 1337;
      private readonly Guid serviceGuid = Guid.NewGuid();
      private readonly string methodName = "the method name";
      private readonly object[] methodArguments = { "test", 3 };

      public C2HServiceInvocationTests() { 
         testObj = new X2XServiceInvocation(
            invocationId, 
            serviceGuid, 
            methodName, 
            methodArguments
         ); 
      }

      [Fact]
      public void PofSerializationTest() {
         PofTestUtilities.CheckConfiguration(new DspPofContext(), testObj);
      }

   }
}
