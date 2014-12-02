using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Dargon.Services.Utilities;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public class ServiceClient : IServiceClient {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceProxyFactory serviceProxyFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConnector connector;

      private readonly IDictionary<Type, object> serviceProxiesByInterface;
      private readonly object synchronization = new object();

      public ServiceClient(ICollectionFactory collectionFactory, IServiceProxyFactory serviceProxyFactory, IServiceContextFactory serviceContextFactory, IConnector connector) {
         this.collectionFactory = collectionFactory;
         this.serviceProxyFactory = serviceProxyFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.connector = connector;

         this.serviceProxiesByInterface = collectionFactory.CreateDictionary<Type, object>();
      }

      public TService GetService<TService>() where TService : class {
         lock (synchronization) {
            var serviceType = typeof(TService);
            object serviceProxy;
            if (!serviceProxiesByInterface.TryGetValue(serviceType, out serviceProxy)) {
               var serviceContext = serviceContextFactory.Create(serviceType, connector);
               serviceProxy = serviceProxyFactory.CreateServiceProxy<TService>(serviceContext);
               serviceProxiesByInterface.Add(serviceType, serviceProxy);
            }
            return (TService)serviceProxy;
         }
      }

      public void Dispose() {
         this.connector.Dispose();
      }
   }

   public interface IServiceContext {
      Type ServiceInterface { get; }

      object Invoke(string methodName, object[] methodArguments);
   }

   public class ServiceContext : IServiceContext {
      private readonly ICollectionFactory collectionFactory;
      private readonly Type serviceInterface;
      private readonly IConnector connector;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;
      private readonly Guid serviceGuid;

      public ServiceContext(ICollectionFactory collectionFactory, Type serviceInterface, IConnector connector) {
         this.collectionFactory = collectionFactory;
         this.serviceInterface = serviceInterface;
         this.connector = connector;

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

         return connector.Invoke(serviceGuid, methodName, methodArguments);
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
