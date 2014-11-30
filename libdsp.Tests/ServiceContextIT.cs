using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.Server;
using ItzWarty;
using ItzWarty.Collections;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class ServiceContextIT : NMockitoInstance {
      private readonly ServiceContext testObj;
      
      private readonly IService service = new ServiceImplementation();
      private readonly ICollectionFactory collectionFactory = new CollectionFactory();

      public ServiceContextIT() {
         testObj = new ServiceContext(collectionFactory, service, typeof(IService));
      }

      [Fact]
      public void ExposedMethodsCorrespondsToInterfaceMethodCount() {
         AssertEquals(2, testObj.MethodsByName.Count);
      }

      [Fact]
      public void ParameterlessFunctionInvocable() {
         AssertEquals(3, testObj.HandleInvocation("Parameterless", new object[0]));
      }

      [Fact]
      public void ExplicitImplementationInvocable() {
         AssertEquals("4", testObj.HandleInvocation("ExplicitImplementation", new object[1] { 4 }));
      }

      [Fact]
      public void NotExposedByInterfaceNotInvocable() {
         Assert.Throws<EntryPointNotFoundException>(() => testObj.HandleInvocation("NotExposedByInterface", new object[0]));
      }

      public class ServiceImplementation : IService {
         public int Parameterless() {
            return 3;
         }

         string IService.ExplicitImplementation(int x) {
            return x.ToString();
         }

         public int NotExposedByInterface() {
            return 5;
         }
      }

      [Guid("1EDA8A4F-AD1D-4203-B24D-75BBD50A21AF")]
      public interface IService {
         int Parameterless();
         string ExplicitImplementation(int x);
      }
   }
}
