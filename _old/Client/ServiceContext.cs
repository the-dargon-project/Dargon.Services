using System;
using System.Reflection;
using Dargon.Services.Common;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public class ServiceContext : IServiceContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly Type serviceInterface;
      private readonly IUserInvocationManager invocationManager;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;
      private readonly Guid serviceGuid;

      public ServiceContext(ICollectionFactory collectionFactory, Type serviceInterface, IUserInvocationManager invocationManager) {
         this.collectionFactory = collectionFactory;
         this.serviceInterface = serviceInterface;
         this.invocationManager = invocationManager;

         methodsByName = collectionFactory.CreateMultiValueDictionary<string, MethodInfo>();
         var interfaces = serviceInterface.GetInterfaces().Concat(serviceInterface);
         foreach (var i in interfaces) {
            var methods = i.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods) {
               methodsByName.Add(method.Name, method);
            }
         }

         serviceGuid = AttributeUtilities.GetInterfaceGuid(serviceInterface);
      }

      public Type ServiceInterface { get { return serviceInterface; } }

      public object Invoke(string methodName, object[] methodArguments) {
         if (!ValidateInvocation(methodName, methodArguments)) {
            throw new InvalidOperationException("Invocation validation failed");
         }

         return invocationManager.Invoke(serviceGuid, methodName, methodArguments);
      }

      private bool ValidateInvocation(string methodName, object[] methodArguments) {
         ItzWarty.Collections.HashSet<MethodInfo> candidates;
         if (methodsByName.TryGetValue(methodName, out candidates)) {
            foreach (var candidate in candidates) {
               var parameters = candidate.GetParameters();
               if (parameters.Length != methodArguments.Length) {
                  continue;
               }
               return true;
            }
         }
         return false;
      }
   }
}