using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;

namespace Dargon.Services.Phases.Host {
   public class HostPhase : IPhase {
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
            IThread thread = null;
            thread = threadingProxy.CreateThread(() => SessionThreadEntryPoint(socket, thread), new ThreadCreationOptions { IsBackground = true });
            thread.Start();
         }
         Debug.WriteLine("Exiting Host Phase ListenerThreadEntryPoint");
      }

      internal void SessionThreadEntryPoint(IConnectedSocket socket, IThread thread) {
         Debug.WriteLine("Entering Host Phase SessionThreadEntryPoint");
         IHostSession session = null;
         try {
            session = hostSessionFactory.Create(thread, hostContext, socket);
            sessions.Add(session);
            session.Start();
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