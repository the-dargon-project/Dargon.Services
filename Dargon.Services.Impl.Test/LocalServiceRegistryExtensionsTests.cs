using System;
using System.Runtime.InteropServices;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class LocalServiceRegistryExtensionsTests : NMockitoInstance {
      private const string kInterfaceGuid = "FD2B6CF5-6385-4956-B17D-3940741749D3";

      [Mock] private readonly LocalServiceRegistry localServiceRegistry = null;

      [Fact]
      public void RegisterService_WithGuidfulInterface_HappyPathTest() {
         var serviceImplementation = CreatePlaceholder<GuidfulInterface>();

         localServiceRegistry.RegisterService(serviceImplementation, typeof(GuidfulInterface));

         Verify(localServiceRegistry).RegisterService(serviceImplementation, typeof(GuidfulInterface), Guid.Parse(kInterfaceGuid));
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterService_WithGuidlessInterface_ThrowsExceptionTest() {
         var serviceImplementation = CreatePlaceholder<GuidfulInterface>();

         AssertThrows<ArgumentException>(() => localServiceRegistry.RegisterService(serviceImplementation, typeof(GuidlessInterface)));

         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisterService_WithGuidfulInterface_HappyPathTest() {
         localServiceRegistry.UnregisterService(typeof(GuidfulInterface));

         Verify(localServiceRegistry).UnregisterService(Guid.Parse(kInterfaceGuid));
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void UnregisterService_WithGuidlessInterface_ThrowsExceptionTest() {
         AssertThrows<ArgumentException>(() => localServiceRegistry.UnregisterService(typeof(GuidlessInterface)));

         VerifyNoMoreInteractions();
      }

      public interface GuidlessInterface { }

      [Guid(kInterfaceGuid)]
      public interface GuidfulInterface { }
   }
}
