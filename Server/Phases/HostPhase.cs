using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Sessions;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Server.Phases {
   public class HostPhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IConnectorContext connectorContext;
      private readonly IListenerSocket listenerSocket;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IConcurrentSet<IClientSession> clientSessions;
      private readonly IConcurrentSet<IGuestSession> guestSessions;
      private readonly IHostContext hostContext;
      private readonly IThread listenerThread;
      private bool disposed = false;

      public HostPhase(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, IHostSessionFactory hostSessionFactory, IConnectorContext connectorContext, IListenerSocket listenerSocket, IHostContext hostContext) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.hostSessionFactory = hostSessionFactory;
         this.connectorContext = connectorContext;
         this.listenerSocket = listenerSocket;
         this.hostContext = hostContext;

         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.clientSessions = collectionFactory.CreateConcurrentSet<IClientSession>();
         this.guestSessions = collectionFactory.CreateConcurrentSet<IGuestSession>();
         this.listenerThread = threadingProxy.CreateThread(ListenerThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
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
            var handshake = pofSerializer.Deserialize<X2SHandshake>(socket.GetReader().__Reader);
            if (handshake.Role == Role.Client) {
               var clientSession = hostSessionFactory.CreateClientSession(thread, hostContext, socket);
               session = clientSession;
               clientSessions.Add(clientSession);
               clientSession.Run();
            } else if (handshake.Role == Role.Guest) {
               var guestSession = hostSessionFactory.CreateGuestSession(thread, hostContext, socket);
               session = guestSession;
               guestSessions.Add(guestSession);
               guestSession.Run();
            } else {
               // do nothing
            }
         } catch (SocketException e) {
            logger.Warn(e);
            Debug.WriteLine(e);
         } catch (Exception e) {
            logger.Error(e);
            Debug.WriteLine(e);
         } finally {
            if (session != null) {
               if (session.Role == Role.Client) {
                  clientSessions.Remove((IClientSession)session);
               } else if (session.Role == Role.Guest) {
                  guestSessions.Remove((IGuestSession)session);
               }
            }
         }
         Debug.WriteLine("Exiting Host Phase SessionThreadEntryPoint");
      }

      public void HandleServiceRegistered(IServiceContext serviceContext) {
         // does nothing
      }

      public void HandleServiceUnregistered(IServiceContext serviceContext) {
         // does nothing
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            listenerSocket.Dispose();
            hostContext.Dispose();
            foreach (var session in clientSessions.ToArray()) {
               session.Dispose();
            }
            foreach (var session in guestSessions.ToArray()) {
               session.Dispose();
            }
         }
      }
   }
}