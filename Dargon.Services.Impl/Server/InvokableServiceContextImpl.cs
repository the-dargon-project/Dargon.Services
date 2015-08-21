using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Dargon.Services.Messaging;

namespace Dargon.Services.Server {
   public interface InvokableServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, Type[] genericArguments, object[] arguments);
      object HandleInvocation(string action, PortableObjectBox genericArguments, PortableObjectBox methodArgumentsDto);
   }

   public class InvokableServiceContextImpl : InvokableServiceContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;
      private readonly object serviceImplementation;
      private readonly Type serviceInterface;
      private readonly Guid guid;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;

      public InvokableServiceContextImpl(ICollectionFactory collectionFactory, PortableObjectBoxConverter portableObjectBoxConverter, object serviceImplementation, Type serviceInterface, Guid guid, IMultiValueDictionary<string, MethodInfo> methodsByName) {
         this.collectionFactory = collectionFactory;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
         this.serviceImplementation = serviceImplementation;
         this.serviceInterface = serviceInterface;
         this.guid = guid;
         this.methodsByName = methodsByName;
      }

      public Guid Guid => guid;

      public object HandleInvocation(string action, Type[] genericArguments, object[] arguments) {
         var isGenericInvocation = genericArguments.Any();
         HashSet<MethodInfo> candidates;
         if (methodsByName.TryGetValue(action, out candidates)) {
            foreach (var candidate in candidates) {
               var parameters = candidate.GetParameters();
               if (parameters.Length != arguments.Length) {
                  continue;
               }
               if (isGenericInvocation != candidate.IsGenericMethod) {
                  continue;
               }
               if (isGenericInvocation && candidate.GetGenericArguments().Length != genericArguments.Length) {
                  continue;
               }
               var invokedMethod = candidate;
               if (isGenericInvocation) {
                  invokedMethod = candidate.MakeGenericMethod(genericArguments);
               }
               return invokedMethod.Invoke(serviceImplementation, arguments);
            }
         }
         throw new EntryPointNotFoundException("Could not find method " + action + " with given arguments");
      }

      public object HandleInvocation(string action, PortableObjectBox genericArgumentsDto, PortableObjectBox methodArgumentsDto) {
         Type[] genericArguments;
         object[] methodArguments;
         if (portableObjectBoxConverter.TryConvertFromDataTransferObject(genericArgumentsDto, out genericArguments) &&
             portableObjectBoxConverter.TryConvertFromDataTransferObject(methodArgumentsDto, out methodArguments)) {
            return HandleInvocation(action, genericArguments, methodArguments);
         } else {
            throw new PortableException(new Exception("Could not deserialize data in argument dto."));
         }
      }
   }
}