using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dargon.Services.Networking.Server;

namespace Dargon.Services.Networking {
   public interface IConnectorFactory {
      IClientConnector CreateClientConnector(IServiceEndpoint endpoint);
      IConnector CreateServiceConnector(int port);
   }
}
