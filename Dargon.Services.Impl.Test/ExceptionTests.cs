using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Ryu;
using Dargon.Services.Messaging;
using NMockito;
using Xunit;
using System.Runtime.InteropServices;

namespace Dargon.Services {
   public class ExceptionTests : NMockitoInstance {
      private const int kTestServicePort = 30003;

      [Fact]
      public void Run() {
         var implementation = CreateMock<TestInterface>();

         var ryu = new RyuFactory().Create();
         ryu.Setup();
         ryu.Get<IPofContext>().RegisterPortableObjectType(1337, typeof(AlternateException));

         var serviceClientFactory = ryu.Get<ServiceClientFactory>();
         var serverServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.HostOnly);
         serverServiceClient.RegisterService(implementation, typeof(TestInterface));

         var clientServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.GuestOnly);
         var remoteService = clientServiceClient.GetService<TestInterface>();

         When(implementation.Exec()).ThenReturn(3).ThenThrow(new InvalidOperationException()).ThenThrow(new AlternateException("It broke!"));
         AssertEquals(3, remoteService.Exec());
         AssertThrows<PortableException>(() => remoteService.Exec());
         AssertThrows<AlternateException>(() => remoteService.Exec());
      }

      [Guid("035A2197-729D-4F48-BECD-E7523970EB67")]
      public interface TestInterface {
         int Exec();
      }

      public class AlternateException : Exception, IPortableObject {
         private string message;

         public AlternateException() { }

         public AlternateException(string message) {
            this.message = message;
         }

         public override string Message => message;

         public void Serialize(IPofWriter writer) {
            writer.WriteString(0, message);
         }

         public void Deserialize(IPofReader reader) {
            message = reader.ReadString(0);
         }
      }
   }
}
