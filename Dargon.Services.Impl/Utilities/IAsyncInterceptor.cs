using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Dargon.Services.Utilities {
   public interface IAsyncInterceptor : IInterceptor {
      Task<object> InterceptAsync(MethodInfo methodInfo, object[] methodArguments);
   }
}
