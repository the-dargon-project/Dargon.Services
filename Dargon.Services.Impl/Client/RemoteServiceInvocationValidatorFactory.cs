using System;
using System.Reflection;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public interface RemoteServiceInvocationValidatorFactory {
      RemoteServiceInvocationValidator Create(Guid serviceGuid, Type serviceInterface);
   }

   public class RemoteServiceInvocationValidatorFactoryImpl : RemoteServiceInvocationValidatorFactory {
      private readonly ICollectionFactory collectionFactory;

      public RemoteServiceInvocationValidatorFactoryImpl(ICollectionFactory collectionFactory) {
         this.collectionFactory = collectionFactory;
      }

      public RemoteServiceInvocationValidator Create(Guid serviceGuid, Type serviceInterface) {
         var methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var interfaces = serviceInterface.GetInterfaces().Concat(serviceInterface);
         foreach (var i in interfaces) {
            var methods = i.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods) {
               methodsByName.Add(method.Name, method);
            }
         }

         return new RemoteServiceInvocationValidatorImpl(serviceInterface, serviceGuid, methodsByName);
      }
   }
}