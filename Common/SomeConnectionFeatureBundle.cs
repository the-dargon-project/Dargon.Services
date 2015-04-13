using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects.Streams;

namespace Dargon.Services.Common {
   public interface SomeConnectionFeatureBundle {
      void ConfigureDispatcher(PofDispatcher dispatcher);
   }
}
