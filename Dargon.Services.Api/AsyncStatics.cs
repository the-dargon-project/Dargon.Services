using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dargon.Services {
   public static class AsyncStatics {
      private static AsyncServiceInvoker asyncServiceInvoker;

      public static void __SetInvokerIfUninitialized(Func<AsyncServiceInvoker> factory) {
         asyncServiceInvoker = asyncServiceInvoker ?? factory();
      }

      public static Task Async(Expression<Action> expression) {
         return asyncServiceInvoker.EvaluateAsync(expression.Body);
      }

      public async static Task<TResult> Async<TResult>(Expression<Func<TResult>> expression) {
         var result = await asyncServiceInvoker.EvaluateAsync(expression.Body);
         return (TResult)result;
      }
   }
}
