using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Clustering.Local.Host {
   public class HostPhase : ClusteringPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly HostSessionFactory hostSessionFactory;
      private readonly HostContext hostContext;
      private readonly IListenerSocket listenerSocket;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IThread listenerThread;
      private readonly IConcurrentSet<HostSession> sessions;
      private bool disposed = false;

      public HostPhase(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, HostSessionFactory hostSessionFactory, HostContext hostContext, IListenerSocket listenerSocket) {
         this.threadingProxy = threadingProxy;
         this.hostSessionFactory = hostSessionFactory;
         this.hostContext = hostContext;
         this.listenerSocket = listenerSocket;

         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.listenerThread = threadingProxy.CreateThread(ListenerThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
         sessions = collectionFactory.CreateConcurrentSet<HostSession>();
      }

      public void HandleEnter() {
         listenerThread.Start();
      }

      internal void ListenerThreadEntryPoint() {
         logger.Info("Entering Host Phase ListenerThreadEntryPoint");
         while (!cancellationTokenSource.IsCancellationRequested) {
            var socket = listenerSocket.Accept();
            var asyncTask = ProcessSessionAsync(socket);
         }
         logger.Info("Exiting Host Phase ListenerThreadEntryPoint");
      }

      internal async Task ProcessSessionAsync(IConnectedSocket socket) {
         logger.Info("Entering Host Phase SessionThreadEntryPoint");
         HostSession session = null;
         try {
            session = hostSessionFactory.Create(hostContext, socket);
            sessions.Add(session);
            await session.StartAndAwaitShutdown();
         } catch (SocketException e) {
            logger.Warn(e);
         } catch (Exception e) {
            logger.Error(e);
         } finally {
            sessions.Remove(session);
         }
         logger.Info("Exiting Host Phase SessionThreadEntryPoint");
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         // does nothing
      }

      public Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments) {
         logger.Trace($"Invoking service call on service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments.");
         return hostContext.Invoke(serviceGuid, methodName, genericArguments, methodArguments);
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