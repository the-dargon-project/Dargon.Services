using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.Collections;

namespace Dargon.Services.Networking {
   public interface IServiceConnectorWorkerFactory {
      IServiceConnectorWorker Create(IConcurrentDictionary<string, IServiceContext> serviceContextsByName);
   }
}
