using Dargon.Services.PortableObjects;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.PortableObjects {
   public class X2SHandshakeTests : NMockitoInstance {
      private readonly X2SHandshake testObj;
      private const Role kClientRole = Role.Guest;

      public X2SHandshakeTests() {
         testObj = new X2SHandshake(kClientRole);
      }

      [Fact]
      public void PofSerializationTest() {
         PofTestUtilities.CheckConfiguration(new DspPofContext(), testObj);
      }

      [Fact]
      public void ClientRoleReflectsConstructorParameterTest() {
         AssertEquals(kClientRole, testObj.Role);
         VerifyNoMoreInteractions();
      }
   }
}
