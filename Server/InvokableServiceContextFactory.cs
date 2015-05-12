using ItzWarty.Collections;
using System;
using Dargon.Services.Messaging;

namespace Dargon.Services.Server {
   public interface InvokableServiceContextFactory {
      InvokableServiceContext Create(object serviceImplementation, Type serviceInterface);
   }

   public class InvokableServiceContextFactoryImpl : InvokableServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly MethodArgumentsConverter methodArgumentsConverter;

      public InvokableServiceContextFactoryImpl(ICollectionFactory collectionFactory, MethodArgumentsConverter methodArgumentsConverter) {
         this.collectionFactory = collectionFactory;
         this.methodArgumentsConverter = methodArgumentsConverter;
      }

      public InvokableServiceContext Create(object serviceImplementation, Type serviceInterface) {
         return new InvokableServiceContextImpl(collectionFactory, methodArgumentsConverter, serviceImplementation, serviceInterface);
      }
   }
}