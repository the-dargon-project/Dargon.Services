using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services.Networking {
   interface IServiceConnectorPhaseFactory {
      IServiceConnectorPhase CreateIndeterminatePhase();
      IServiceConnectorPhase CreateHostPhase();
      IServiceConnectorPhase CreateClientPhase();
   }
}
