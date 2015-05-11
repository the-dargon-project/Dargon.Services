using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Data;
using System.Reflection;
using Dargon.Services.Messaging;

namespace Dargon.Services.Server {
   public interface InvokableServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, object[] arguments);
      object HandleInvocation(string action, MethodArgumentsDto arguments);
   }

   public class InvokableServiceContextImpl : InvokableServiceContext {
      private readonly MethodArgumentsConverter methodArgumentsConverter;
      private readonly object serviceImplementation;
      private readonly Type serviceInterface;
      private readonly Guid guid;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;

      public InvokableServiceContextImpl(ICollectionFactory collectionFactory, MethodArgumentsConverter methodArgumentsConverter, object serviceImplementation, Type serviceInterface) {
         this.methodArgumentsConverter = methodArgumentsConverter;
         this.serviceImplementation = serviceImplementation;
         this.serviceInterface = serviceInterface;

         guid = AttributeUtilities.GetInterfaceGuid(serviceInterface);

         methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var interfaces = serviceInterface.GetInterfaces().Concat(serviceInterface);
         foreach (var i in interfaces) {
            var interfaceMethods = i.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in interfaceMethods) {
               methodsByName.Add(method.Name, method);
            }
         }
      }

      public Guid Guid => guid;

      public object HandleInvocation(string action, object[] arguments) {
         HashSet<MethodInfo> candidates;
         if (methodsByName.TryGetValue(action, out candidates)) {
            foreach (var candidate in candidates) {
               var parameters = candidate.GetParameters();
               if (parameters.Length != arguments.Length) {
                  continue;
               }
               return candidate.Invoke(serviceImplementation, arguments);
            }
         }
         throw new EntryPointNotFoundException("Could not find method " + action + " with given arguments");
      }

      public object HandleInvocation(string action, MethodArgumentsDto arguments) {
         object[] methodArguments;
         if (methodArgumentsConverter.TryConvertFromDataTransferObject(arguments, out methodArguments)) {
            return HandleInvocation(action, methodArguments);
         } else {
            throw new PortableException(new Exception("Could not deserialize data in argument dto."));
         }
      }
   }
}