using ItzWarty.Threading;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dargon.Services.Utilities {
   public interface AsyncValueBox {
      void SetResult(object value);
      void SetException(Exception value);
      Task<object> GetResultAsync();
      Task<object> GetResultAsync(ICancellationToken cancellationToken);
   }

   public class AsyncValueBoxImpl : AsyncValueBox {
      private readonly object synchronization = new object();

      // Hack: Seems like Nito AsyncEx doesn't have a manual reset event w/ async waiting?
      private readonly SemaphoreSlim countdown;
      private object result;
      private bool isExceptionThrown = false;
      private bool isResultSet = false;

      public AsyncValueBoxImpl() {
         this.countdown = new SemaphoreSlim(0);
         this.result = null;
      }

      public void SetResult(object value) {
         lock (synchronization) {
            if (!isResultSet) {
               result = value;
               isExceptionThrown = false;
               isResultSet = true;
               Thread.MemoryBarrier();
               countdown.Release(int.MaxValue);
            }
         }
      }

      public void SetException(Exception ex) {
         lock (synchronization) {
            if (!isResultSet) {
               result = ex;
               isExceptionThrown = true;
               isResultSet = true;
               Thread.MemoryBarrier();
               countdown.Release(int.MaxValue);
            }
         }
      }

      public async Task<object> GetResultAsync() {
         await countdown.WaitAsync();
         return GetResultHelper();
      }

      public async Task<object> GetResultAsync(ICancellationToken cancellationToken) {
         await countdown.WaitAsync(cancellationToken.__InnerToken);
         return GetResultHelper();
      }

      private object GetResultHelper() {
         if (isExceptionThrown) {
            ExceptionDispatchInfo.Capture((Exception)result).Throw();
            throw new Exception("Unreachable code.");
         } else {
            return result;
         }
      }
   }
}
