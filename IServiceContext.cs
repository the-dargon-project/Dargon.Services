using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dargon.Services {
   public interface IServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, object[] arguments);
   }
}
