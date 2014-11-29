using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.Collections;
using NMockito;

namespace Dargon.Services {
   public class ServiceContextTests : NMockitoInstance {
      private readonly ServiceContext testObj;
      
      private readonly IService service = new ServiceImplementation();
      [Mock] private readonly ICollectionFactory collectionFactory;

      public ServiceContextTests() {
         testObj = new ServiceContext(collectionFactory, service, typeof(IService));
      }

      public class ServiceImplementation : IService {
         public int Parameterless() {
            return 3;
         }

         string IService.ExplicitImplementation(int x) {
            return x.ToString();
         }
      }

      public interface IService {
         int Parameterless();
         string ExplicitImplementation(int x);
      }
   }
}
