using System;
using System.Linq;
using System.Reflection;
using Dargon.Services.Messaging;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;

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
         var methodDescriptorsByName = collectionFactory.CreateMultiValueDictionary<string, MethodDescriptor>();
         var interfaces = serviceInterface.GetInterfaces().Concat(serviceInterface);
         foreach (var i in interfaces) {
            var interfaceMethods = i.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in interfaceMethods) {
               var outRefParameterIndices = methodInfo.GetOutRefParameterIndices();
               var methodDescriptor = new MethodDescriptor(methodInfo, outRefParameterIndices);
               methodDescriptorsByName.Add(methodInfo.Name, methodDescriptor);
            }
         }
         return new InvokableServiceContextImpl(collectionFactory, portableObjectBoxConverter, serviceImplementation, serviceInterface, guid, methodDescriptorsByName);
      }
   }
}
