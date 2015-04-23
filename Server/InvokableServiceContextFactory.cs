using System;
using System.Runtime.Remoting.Messaging;
using ItzWarty.Collections;

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