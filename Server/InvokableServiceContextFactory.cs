using ItzWarty.Collections;
using System;

namespace Dargon.Services.Server {
   public interface InvokableServiceContextFactory {
      InvokableServiceContext Create(object serviceImplementation, Type serviceInterface);
   }

   public class InvokableServiceContextFactoryImpl : InvokableServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;

      public InvokableServiceContextFactoryImpl(ICollectionFactory collectionFactory) {
         this.collectionFactory = collectionFactory;
      }

      public InvokableServiceContext Create(object serviceImplementation, Type serviceInterface) {
         return new InvokableServiceContextImpl(collectionFactory, serviceImplementation, serviceInterface);
      }
   }
}