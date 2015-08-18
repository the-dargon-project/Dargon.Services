using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class RyuContainerTests : NMockitoInstance {
      public RyuContainerTests() {

      }

      [Fact]
      public void Run() {
         Console.WriteLine(typeof(IServiceClient).FullName);
         var ryu = new RyuFactory().Create();
         ryu.Setup();

//         var clusteringConfiguration = CreateMock<IClusteringConfiguration>();
//         ryu.Set<IClusteringConfiguration>(clusteringConfiguration);

//         var serviceClient = ryu.Get<IServiceClient>();
//         AssertNotNull(serviceClient);
      }
   }
}
