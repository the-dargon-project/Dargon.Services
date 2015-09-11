using Dargon.Services.Messaging;
using ItzWarty.Collections;
using System;
using System.Linq;
using ItzWarty;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Dargon.Services.Messaging;
using ItzWarty.Collections;
using Nito.AsyncEx;

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
      private readonly IMultiValueDictionary<string, MethodDescriptor> methodDescriptorsByName;

      public InvokableServiceContextImpl(ICollectionFactory collectionFactory, PortableObjectBoxConverter portableObjectBoxConverter, object serviceImplementation, Type serviceInterface, Guid guid, IMultiValueDictionary<string, MethodDescriptor> methodDescriptorsByName) {
         this.collectionFactory = collectionFactory;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
         this.serviceImplementation = serviceImplementation;
         this.serviceInterface = serviceInterface;
         this.guid = guid;
         this.methodDescriptorsByName = methodDescriptorsByName;
      }

      public Guid Guid => guid;

      public object HandleInvocation(string action, Type[] genericArguments, object[] arguments) {
         var isGenericInvocation = genericArguments.Any();
         HashSet<MethodDescriptor> methodDescriptors;
         if (methodDescriptorsByName.TryGetValue(action, out methodDescriptors)) {
            foreach (var methodDescriptor in methodDescriptors) {
               var methodInfo = methodDescriptor.MethodInfo;
               var parameters = methodInfo.GetParameters();
               if (parameters.Length != arguments.Length) {
                  continue;
               }
               if (isGenericInvocation != methodInfo.IsGenericMethod) {
                  continue;
               }
               if (isGenericInvocation && methodInfo.GetGenericArguments().Length != genericArguments.Length) {
                  continue;
               }
               var invokedMethod = methodInfo;
               if (isGenericInvocation) {
                  invokedMethod = methodInfo.MakeGenericMethod(genericArguments);
               }
               object returnValue;
               try {
                  returnValue = invokedMethod.Invoke(serviceImplementation, arguments);
               } catch (TargetInvocationException tie) {
                  ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                  throw new InvalidOperationException(nameof(InvokableServiceContextImpl) + " Unreachable Code");
               }
               if (methodDescriptor.OutRefParameterIndices.None()) {
                  return returnValue;
               } else {
                  return new OutRefMethodResult(
                     returnValue,
                     Util.Generate(
                        methodDescriptor.OutRefParameterIndices.Length,
                        i => arguments[methodDescriptor.OutRefParameterIndices[i]]));
               }
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
