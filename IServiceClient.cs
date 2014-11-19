using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Services
{
   public interface IServiceClient
   {
      TService GetService<TService>(IServiceEndpoint endpoint) where TService : class;
   }
}
