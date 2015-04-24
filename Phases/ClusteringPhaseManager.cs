using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services.Server;

namespace Dargon.Services.Phases {
   public interface ClusteringPhaseManager : IDisposable {
      void Transition(IPhase nextPhase);
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);
   }

   public class ClusteringPhaseManagerImpl : ClusteringPhaseManager {
      private readonly object synchronization = new object();
      private bool disposed = false;
      private IPhase currentPhase = null;

      public void Transition(IPhase nextPhase) {
         lock (synchronization) {
            ThrowIfDisposed();

            Debug.WriteLine("Transition from phase " + (currentPhase == null ? "null" : currentPhase.ToString()) + " to " + (nextPhase == null ? "null" : nextPhase.ToString()));
            currentPhase = nextPhase;
            currentPhase.HandleEnter();
         }
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            currentPhase.HandleServiceRegistered(invokableServiceContext);
         }
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         lock (synchronization) {
            currentPhase.HandleServiceUnregistered(invokableServiceContext);
         }
      }

      public void Dispose() {
         lock (synchronization) {
            if (!disposed) {
               disposed = true;
               currentPhase.Dispose();
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
