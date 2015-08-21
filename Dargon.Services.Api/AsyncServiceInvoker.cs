using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dargon.Services {
   public interface AsyncServiceInvoker {
      Task<object> EvaluateAsync(Expression expression);
   }
}