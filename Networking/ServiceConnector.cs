using System;
using System.Collections.Generic;
using ItzWarty.Collections;

namespace Dargon.Services.Networking {
   public class ServiceConnector : IServiceConnector, IDisposable {
      private readonly IServiceConnectorWorkerFactory serviceConnectorWorkerFactory;
      private readonly IConcurrentDictionary<string, IServiceContext> serviceContextsByName;
      private readonly object contextLock = new object();
      private IServiceConnectorWorker serviceConnectorWorker;

      public ServiceConnector(IServiceConnectorWorkerFactory serviceConnectorWorkerFactory) : this(serviceConnectorWorkerFactory, new ConcurrentDictionary<string, IServiceContext>()) { }

      internal ServiceConnector(IServiceConnectorWorkerFactory serviceConnectorWorkerFactory, IConcurrentDictionary<string, IServiceContext> serviceContextsByName) {
         this.serviceConnectorWorkerFactory = serviceConnectorWorkerFactory;
         this.serviceContextsByName = serviceContextsByName;
      }

      public void Initialize() {
         serviceConnectorWorker = serviceConnectorWorkerFactory.Create(serviceContextsByName);
         serviceConnectorWorker.Start();
      }

      public void RegisterService(IServiceContext serviceContext) {
         lock (contextLock) {
            if (!this.serviceContextsByName.TryAdd(serviceContext.Name, serviceContext)) {
               throw new InvalidOperationException("Attempted to register service context twice!");
            } else {
               serviceConnectorWorker.SignalUpdate();
            }
         }
      }

      public void UnregisterService(IServiceContext serviceContext) {
         lock (contextLock) {
            IServiceContext removedContext;
            if (!this.serviceContextsByName.TryRemove(serviceContext.Name, out removedContext)) {
               throw new InvalidOperationException("Attempted to deregister unregistered service context!");
            } else if (removedContext != serviceContext) {
               throw new InvalidOperationException("ServiceContext of name " + serviceContext.Name + " removed, but references did not match!?");
            } else {
               serviceConnectorWorker.SignalUpdate();
            }
         }
      }

      public void Dispose() {
         if (serviceConnectorWorker != null) {
            serviceConnectorWorker.Dispose();
         }
      }
   }
}