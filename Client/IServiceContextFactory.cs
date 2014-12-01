using System;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public interface IServiceContextFactory {
      IServiceContext Create(Type serviceInterface, IConnector connector);
   }

   public class ServiceContextFactory : IServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;

      public ServiceContextFactory(ICollectionFactory collectionFactory) {
         this.collectionFactory = collectionFactory;
      }

      public IServiceContext Create(Type serviceInterface, IConnector connector) {
         return new ServiceContext(collectionFactory, serviceInterface, connector);
      }
   }
}