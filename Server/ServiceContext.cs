using System;
using System.Reflection;
using Dargon.Services.Utilities;
using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public class ServiceContext : IServiceContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly object serviceImplementation;
      private readonly Type serviceInterface;
      private readonly Guid guid;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;

      public ServiceContext(ICollectionFactory collectionFactory, object serviceImplementation, Type serviceInterface) {
         this.collectionFactory = collectionFactory;
         this.serviceImplementation = serviceImplementation;
         this.serviceInterface = serviceInterface;

         guid = AttributeUtilities.GetInterfaceGuid(serviceInterface);

         methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var interfaceMethods = serviceInterface.GetMethods(BindingFlags.Instance | BindingFlags.Public);
         foreach (var method in interfaceMethods) {
            methodsByName.Add(method.Name, method);
         }
      }

      public IMultiValueDictionary<string, MethodInfo> MethodsByName { get { return methodsByName; } } 

      public Guid Guid { get { return guid; } }

      public object HandleInvocation(string action, object[] arguments) {
         HashSet<MethodInfo> candidates;
         if (methodsByName.TryGetValue(action, out candidates)) {
            foreach (var candidate in candidates) {
               var parameters = candidate.GetParameters();
               if (parameters.Length != arguments.Length) {
                  break;
               }
               return candidate.Invoke(serviceImplementation, arguments);
            }
         }
         throw new EntryPointNotFoundException("Could not find method " + action + " with given arguments");
      }
   }
}