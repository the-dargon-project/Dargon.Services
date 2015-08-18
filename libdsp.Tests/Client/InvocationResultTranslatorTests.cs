using System;
using Dargon.Services.Messaging;
using NMockito;
using Xunit;

namespace Dargon.Services.Client {
   public class InvocationResultTranslatorTests : NMockitoInstance {
      [Mock] private readonly PortableObjectBoxConverter portableObjectBoxConverter = null;
      private readonly InvocationResultTranslatorImpl testObj;

      public InvocationResultTranslatorTests() {
         testObj = new InvocationResultTranslatorImpl(portableObjectBoxConverter);
      }

      [Fact]
      public void TranslateOrThrow_GivenNonboxedResult_ReturnsResult() {
         const string kValue = "test_string";
         var result = testObj.TranslateOrThrow(kValue, kValue.GetType());
         AssertEquals(result, kValue);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void TranslateOrThrow_GivenException_ThrowsException() {
         var exception = new Exception();
         var expectedReturnType = typeof(void);
         AssertThrows<Exception>(() => testObj.TranslateOrThrow(exception, expectedReturnType));
         VerifyNoMoreInteractions();
      }
   }
}