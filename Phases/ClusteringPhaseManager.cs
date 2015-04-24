using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.Server;

namespace Dargon.Services.Phases {
   public interface ClusteringPhaseManager : IDisposable {
      void Transition(ClusteringPhase nextClusteringPhase);
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }

   public class ClusteringPhaseManagerImpl : ClusteringPhaseManager {
      private readonly object synchronization = new object();
      private bool disposed = false;
      private ClusteringPhase currentClusteringPhase = null;

      public void Transition(ClusteringPhase nextClusteringPhase) {
         lock (synchronization) {
            ThrowIfDisposed();

            Debug.WriteLine("Transition from phase " + (currentClusteringPhase == null ? "null" : currentClusteringPhase.ToString()) + " to " + (nextClusteringPhase == null ? "null" : nextClusteringPhase.ToString()));
            currentClusteringPhase = nextClusteringPhase;
            currentClusteringPhase.HandleEnter();
         }
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            currentClusteringPhase.HandleServiceRegistered(invokableServiceContext);
         }
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            currentClusteringPhase.HandleServiceUnregistered(invokableServiceContext);
         }
      }

      public void Dispose() {
         lock (synchronization) {
            if (!disposed) {
               disposed = true;
               currentClusteringPhase.Dispose();
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
