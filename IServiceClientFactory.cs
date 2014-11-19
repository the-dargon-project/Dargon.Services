using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dargon.Services
{
   public interface IServiceClientFactory
   {
      IServiceClient Create();
   }
}
