using System;
using System.Collections.Generic;
using Dargon.Services.Messaging;
using ItzWarty;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Services.Server {
   public interface LocalServiceContainer {
      event Action<InvokableServiceContext> ServiceRegistered;
      event Action<InvokableServiceContext> ServiceUnregistered;

      void Register(InvokableServiceContext invokableServiceContext);
      void Unregister(InvokableServiceContext invokableServiceContext);

      IEnumerable<Guid> EnumerateServiceGuids();

      bool TryInvoke(Guid serviceGuid, string methodName, PortableObjectBox genericArgumentsDto, PortableObjectBox methodArgumentsDto, out object result);
      bool TryInvoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments, out object result);

   }

   public class LocalServiceContainerImpl : LocalServiceContainer {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly object synchronization = new object();

      private readonly IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsByGuid;

      public LocalServiceContainerImpl(
         ICollectionFactory collectionFactory
      ) : this(collectionFactory.CreateConcurrentDictionary<Guid, InvokableServiceContext>()) {
      }

      public LocalServiceContainerImpl(IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsByGuid) {
         this.serviceContextsByGuid = serviceContextsByGuid;
      }

      public event Action<InvokableServiceContext> ServiceRegistered;
      public event Action<InvokableServiceContext> ServiceUnregistered;

      public void Register(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            if (serviceContextsByGuid.TryAdd(invokableServiceContext.Guid, invokableServiceContext)) {
               ServiceRegistered?.Invoke(invokableServiceContext);
            }
         }
      }

      public void Unregister(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            var kvp = invokableServiceContext.Guid.PairValue(invokableServiceContext);
            if (serviceContextsByGuid.Remove(kvp)) {
               ServiceUnregistered?.Invoke(invokableServiceContext);
            }
         }
      }

      public IEnumerable<Guid> EnumerateServiceGuids() {
         return serviceContextsByGuid.Keys;
      }

      public bool TryInvoke(Guid serviceGuid, string methodName, PortableObjectBox genericArgumentsDto, PortableObjectBox methodArgumentsDto, out object result) {
         InvokableServiceContext invokableServiceContext;
         if (!serviceContextsByGuid.TryGetValue(serviceGuid, out invokableServiceContext)) {
            result = null;
            return false;
         } else {
            result = invokableServiceContext.HandleInvocation(methodName, genericArgumentsDto, methodArgumentsDto);
            return true;
         }
      }

      public bool TryInvoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments, out object result) {
         InvokableServiceContext invokableServiceContext;
         if (!serviceContextsByGuid.TryGetValue(serviceGuid, out invokableServiceContext)) {
            result = null;
            return false;
         } else {
            result = invokableServiceContext.HandleInvocation(methodName, genericArguments, methodArguments);
            return true;
         }
      }
   }
}