using System;
using Dargon.Services.Common;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public interface IServiceContextFactory {
      IServiceContext Create(Type serviceInterface, IUserInvocationManager invocationManager);
   }

   public class ServiceContextFactory : IServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;

      public ServiceContextFactory(ICollectionFactory collectionFactory) {
         this.collectionFactory = collectionFactory;
      }

      public IServiceContext Create(Type serviceInterface, IUserInvocationManager invocationManager) {
         return new ServiceContext(collectionFactory, serviceInterface, invocationManager);
      }
   }
}