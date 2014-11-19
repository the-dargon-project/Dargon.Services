using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dargon.Services.Networking {
   public interface IServiceConnector {
      void RegisterService(IServiceContext context);
      void UnregisterService(IServiceContext serviceContext);
   }
}
