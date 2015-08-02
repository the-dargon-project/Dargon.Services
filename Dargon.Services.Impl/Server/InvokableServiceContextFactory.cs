using ItzWarty.Collections;
using System;
using Dargon.Services.Messaging;

namespace Dargon.Services.Server {
   public interface InvokableServiceContextFactory {
      InvokableServiceContext Create(object serviceImplementation, Type serviceInterface);
   }

   public class InvokableServiceContextFactoryImpl : InvokableServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public InvokableServiceContextFactoryImpl(ICollectionFactory collectionFactory, PortableObjectBoxConverter portableObjectBoxConverter) {
         this.collectionFactory = collectionFactory;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }

      public InvokableServiceContext Create(object serviceImplementation, Type serviceInterface) {
         return new InvokableServiceContextImpl(collectionFactory, portableObjectBoxConverter, serviceImplementation, serviceInterface);
      }
   }
}