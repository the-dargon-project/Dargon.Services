using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dargon.Services.Phases;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Services.Server {
   public interface LocalServiceContainer : IDisposable {
      IClusteringConfiguration ClusteringConfiguration { get; }
      IPhase CurrentPhase { get; }

      bool TryInvoke(Guid serviceGuid, string methodName, object[] methodArguments, out object result);
      IEnumerable<Guid> EnumerateServiceGuids();

      void Transition(IPhase phase);

      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }

   public class LocalServiceContainerImpl : LocalServiceContainer {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IClusteringConfiguration clusteringConfiguration;
      private readonly IConcurrentDictionary<Guid, InvokableServiceContext> serviceContextsByGuid;
      private readonly object synchronization = new object();
      private IPhase phase;
      private bool disposed = false;

      public LocalServiceContainerImpl(ICollectionFactory collectionFactory, IClusteringConfiguration clusteringConfiguration) {
         this.clusteringConfiguration = clusteringConfiguration;
         this.serviceContextsByGuid = collectionFactory.CreateConcurrentDictionary<Guid, InvokableServiceContext>();
      }

      public IClusteringConfiguration ClusteringConfiguration { get { return clusteringConfiguration; } }
      public IPhase CurrentPhase { get { return phase; } }

      public void Transition(IPhase phase) {
         lock (synchronization) {
            ThrowIfDisposed();

            Debug.WriteLine("Transition from phase " + (this.phase == null ? "null" : this.phase.ToString()) + " to " + (phase == null ? "null" : phase.ToString()));
            this.phase = phase;
            this.phase.HandleEnter();
         }
      }

      public IEnumerable<Guid> EnumerateServiceGuids() {
         return serviceContextsByGuid.Keys;
      }

      public bool TryInvoke(Guid serviceGuid, string methodName, object[] methodArguments, out object result) {
         InvokableServiceContext invokableServiceContext;
         if (!serviceContextsByGuid.TryGetValue(serviceGuid, out invokableServiceContext)) {
            result = null;
            return false;
         } else {
            result = invokableServiceContext.HandleInvocation(methodName, methodArguments);
            return true;
         }
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            serviceContextsByGuid.Add(invokableServiceContext.Guid, invokableServiceContext);
            phase.HandleServiceRegistered(invokableServiceContext);
         }
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            serviceContextsByGuid.Remove(new KeyValuePair<Guid, InvokableServiceContext>(invokableServiceContext.Guid, invokableServiceContext));
            phase.HandleServiceUnregistered(invokableServiceContext);
         }
      }

      public void Dispose() {
         lock (synchronization) {
            if (!disposed) {
               disposed = true;
               this.phase.Dispose();
            }
         }
      }

      internal void ThrowIfDisposed() {
         if (disposed) {
            const string error = "This connector context has already been disposed!";
            throw new ObjectDisposedException("this", error);
         }
      }
   }
}