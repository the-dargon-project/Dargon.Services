using Nito.AsyncEx;
using System.Threading.Tasks;

namespace Dargon.Services.Utilities {
   public interface AsyncValueBox {
      void Set(object value);
      Task<object> GetResultAsync();
   }

   public class AsyncValueBoxImpl : AsyncValueBox {
      private AsyncCountdownEvent countdown;
      private object value;

      public AsyncValueBoxImpl() {
         this.countdown = new AsyncCountdownEvent(1);
         this.value = null;
      }

      public AsyncValueBoxImpl(object value) {
         this.countdown = new AsyncCountdownEvent(0);
         this.value = value;
      }

      public void Set(object value) {
         this.value = value;
         countdown.Signal();
      }

      public async Task<object> GetResultAsync() {
         await countdown.WaitAsync();
         return value;
      }
   }
}
