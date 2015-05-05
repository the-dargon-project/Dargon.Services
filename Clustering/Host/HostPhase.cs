using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Clustering.Host {
   public class HostPhase : ClusteringPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IHostContext hostContext;
      private readonly IListenerSocket listenerSocket;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IThread listenerThread;
      private readonly IConcurrentSet<IHostSession> sessions;
      private bool disposed = false;

      public HostPhase(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, IHostSessionFactory hostSessionFactory, IHostContext hostContext, IListenerSocket listenerSocket) {
         this.threadingProxy = threadingProxy;
         this.hostSessionFactory = hostSessionFactory;
         this.hostContext = hostContext;
         this.listenerSocket = listenerSocket;

         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.listenerThread = threadingProxy.CreateThread(ListenerThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
         sessions = collectionFactory.CreateConcurrentSet<IHostSession>();
      }

      public void HandleEnter() {
         listenerThread.Start();
      }

      internal void ListenerThreadEntryPoint() {
         Debug.WriteLine("Entering Host Phase ListenerThreadEntryPoint");
         while (!cancellationTokenSource.IsCancellationRequested) {
            var socket = listenerSocket.Accept();
            var asyncTask = ProcessSessionAsync(socket);
         }
         Debug.WriteLine("Exiting Host Phase ListenerThreadEntryPoint");
      }

      internal async Task ProcessSessionAsync(IConnectedSocket socket) {
         Debug.WriteLine("Entering Host Phase SessionThreadEntryPoint");
         IHostSession session = null;
         try {
            session = hostSessionFactory.Create(hostContext, socket);
            sessions.Add(session);
            await session.StartAndAwaitShutdown();
         } catch (SocketException e) {
            logger.Warn(e);
            Debug.WriteLine(e);
         } catch (Exception e) {
            logger.Error(e);
            Debug.WriteLine(e);
         } finally {
            sessions.Remove(session);
         }
         Debug.WriteLine("Exiting Host Phase SessionThreadEntryPoint");
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, object[] methodArguments) {
         return hostContext.Invoke(serviceGuid, methodName, methodArguments);
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            listenerSocket.Dispose();
            hostContext.Dispose();
            foreach (var session in sessions.ToArray()) {
               session.Dispose();
            }
         }
      }
   }
}