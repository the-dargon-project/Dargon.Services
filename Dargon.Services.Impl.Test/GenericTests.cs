using System.Runtime.InteropServices;
using Dargon.Ryu;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class GenericTests : NMockitoInstance {
      private const int kTestServicePort = 31040;

      [Fact]
      public void Run() {
         var ryu = new RyuFactory().Create();
         ryu.Setup();
         var serviceClientFactory = ryu.Get<ServiceClientFactory>();
         var serverServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.HostOnly);
         var dependency = CreateMock<WrapperClass<int, string>.Dependency<bool>>();
         serverServiceClient.RegisterService(new WrapperClass<int, string>.BoxServiceImpl<bool>(dependency), typeof(WrapperClass<int, string>.BoxService<bool>));
         
         var clientServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.GuestOnly);

         var remoteService = clientServiceClient.GetService<WrapperClass<int, string>.BoxService<bool>>();
         remoteService.Put(10, "hello", true, 123);
         remoteService.Put(10, "hello", true, true);

         Verify(dependency, Once()).Touch(10, "hello", true, 123);
         Verify(dependency, Once(), AfterPrevious()).Touch(10, "hello", true, true);
      }

      public class WrapperClass<TKey, TValue> {
         [Guid("3E8A7413-BAFD-410D-81D3-0AD9404B84E6")]
         public interface BoxService<TOther> {
            TValue Put<TAnother>(TKey key, TValue value, TOther other, TAnother another);
         }

         public class BoxServiceImpl<TOther> : BoxService<TOther> {
            private readonly Dependency<TOther> dependency;

            public BoxServiceImpl(Dependency<TOther> dependency) {
               this.dependency = dependency;
            }

            public TValue Put<TAnother>(TKey key, TValue value, TOther other, TAnother another) {
               dependency.Touch(key, value, other, another);
               return value;
            }
         }

         public interface Dependency<TOther> {
            void Touch<TAnother>(TKey key, TValue value, TOther other, TAnother another);
         }
      }
   }
}
