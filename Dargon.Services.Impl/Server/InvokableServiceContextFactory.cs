using ItzWarty.Collections;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty;

namespace Dargon.Services.Server {
   public interface InvokableServiceContextFactory {
      InvokableServiceContext Create(object serviceImplementation, Type serviceInterface, Guid guid);
   }

   public class InvokableServiceContextFactoryImpl : InvokableServiceContextFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public InvokableServiceContextFactoryImpl(ICollectionFactory collectionFactory, PortableObjectBoxConverter portableObjectBoxConverter) {
         this.collectionFactory = collectionFactory;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }
      
      public InvokableServiceContext Create(object serviceImplementation, Type serviceInterface, Guid guid) {
         var methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var interfaces = serviceInterface.GetInterfaces().Concat(serviceInterface);
         foreach (var i in interfaces) {
            var interfaceMethods = i.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in interfaceMethods) {
               methodsByName.Add(method.Name, method);
            }
         }

         return new InvokableServiceContextImpl(collectionFactory, portableObjectBoxConverter, serviceImplementation, serviceInterface, guid, methodsByName);
      }
   }
}