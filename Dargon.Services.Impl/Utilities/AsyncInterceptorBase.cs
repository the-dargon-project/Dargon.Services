using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Dargon.Services.Utilities {
   public abstract class AsyncInterceptorBase : IAsyncInterceptor {
      public void Intercept(IInvocation invocation) {
         try {
            invocation.ReturnValue = InterceptAsync(invocation.Method, invocation.Arguments).Result;
         } catch (AggregateException ae) {
            if (ae.InnerExceptions.Count == 1) {
               ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
            } else {
               throw;
            }
         }
      }

      public abstract Task<object> InterceptAsync(MethodInfo methodInfo, object[] methodArguments);
   }
}
