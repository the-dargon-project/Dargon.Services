using System;
using System.Runtime.InteropServices;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class RemoteServiceClientContainerTests : NMockitoInstance {
      private const string kInterfaceGuid = "FD2B6CF5-6385-4956-B17D-3940741749D3";

      [Mock] private readonly RemoteServiceProxyContainer remoteServiceProxyContainer = null;

      [Fact]
      public void GetService_WithGuidlessInterface_DelegatesToRemoteServiceProxyContainerTest() {
         AssertThrows<ArgumentException>(() => remoteServiceProxyContainer.GetService<GuidlessInterface>());

         VerifyNoMoreInteractions();
      }

      [Fact]
      public void GetService_WithGuidfulInterface_DelegatesToRemoteServiceProxyContainerTest() {
         var expectedService = CreatePlaceholder<GuidfulInterface>();

         When(remoteServiceProxyContainer.GetService<GuidfulInterface>(Guid.Parse(kInterfaceGuid))).ThenReturn(expectedService);

         var actualService = remoteServiceProxyContainer.GetService<GuidfulInterface>();

         Verify(remoteServiceProxyContainer).GetService<GuidfulInterface>(Guid.Parse(kInterfaceGuid));
         VerifyNoMoreInteractions();

         AssertEquals(expectedService, actualService);
      }

      public interface GuidlessInterface { }

      [Guid(kInterfaceGuid)]
      public interface GuidfulInterface { }
   }
}
