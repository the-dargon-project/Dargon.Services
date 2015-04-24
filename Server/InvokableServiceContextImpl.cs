using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Reflection;

namespace Dargon.Services.Server {
   public interface InvokableServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, object[] arguments);
   }

   public class InvokableServiceContextImpl : InvokableServiceContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly object serviceImplementation;
      private readonly Type serviceInterface;
      private readonly Guid guid;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;

      public InvokableServiceContextImpl(ICollectionFactory collectionFactory, object serviceImplementation, Type serviceInterface) {
         this.collectionFactory = collectionFactory;
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

      public Guid Guid { get { return guid; } }

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
   }
}