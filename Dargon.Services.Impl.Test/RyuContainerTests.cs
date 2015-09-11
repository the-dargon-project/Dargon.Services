using System;
using Dargon.Ryu;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class RyuContainerTests : NMockitoInstance {
      [Fact]
      public void Run() {
         Console.WriteLine(typeof(ServiceClient).FullName);
         var ryu = new RyuFactory().Create();
         ryu.Setup();

//         var clusteringConfiguration = CreateMock<IClusteringConfiguration>();
//         ryu.Set<IClusteringConfiguration>(clusteringConfiguration);

//         var serviceClient = ryu.Get<IServiceClient>();
//         AssertNotNull(serviceClient);
      }
   }
}
