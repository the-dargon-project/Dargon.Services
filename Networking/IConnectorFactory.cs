using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dargon.Services.Networking {
   public interface IConnectorFactory {
      IClientConnector CreateClientConnector(IServiceEndpoint endpoint);
      IServiceConnector CreateServiceConnector(int port);
   }
}
