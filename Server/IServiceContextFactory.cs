using System;
using System.Runtime.Remoting.Messaging;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public interface IServiceContextFactory {
      IServiceContext Create(object serviceImplementation, Type serviceInterface);
   }

   public class ServiceContextFactory : IServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;

      public ServiceContextFactory(ICollectionFactory collectionFactory) {
         this.collectionFactory = collectionFactory;
      }

      public IServiceContext Create(object serviceImplementation, Type serviceInterface) {
         return new ServiceContext(collectionFactory, serviceImplementation, serviceInterface);
      }
   }
}