using ItzWarty.Collections;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dargon.Services {
   public interface IServiceContext {
      Guid Guid { get; }
      object HandleInvocation(string action, object[] arguments);
   }

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

         var guidAttribute = (GuidAttribute)serviceInterface.GetTypeInfo().GetCustomAttributes(typeof(GuidAttribute), false)[0];
         guid = Guid.Parse(guidAttribute.Value);

         var type = serviceImplementation.GetType();
         methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var methods = type.GetMethods(BindingFlags.Instance);
         foreach (var method in methods) {
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
